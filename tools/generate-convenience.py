#!/usr/bin/env python3
"""Generate convenience records, enums, interfaces and golden tests from tools/model-specs/.

Idempotent: a second run must produce an empty git diff.

Spec shape: options.request is the flat option list shared by all tasks of a family;
model_params.json maps family -> UI parameter list, whose default wins on conflict.
Type map: int->long?, float->double?, bool->bool?, enum-with-values->generated enum?,
everything else (enums without values, seed, path/list types)->string?.
BuildOptions emits an option only when set and != resolved default (wire-equivalent:
the server applies the spec default itself). Validate checks task gates, required
fields and numeric bounds; enum membership is enforced by the property type.
"""

import glob
import json
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SPECS_DIR = os.path.join(ROOT, "tools", "model-specs")
TASKS = ("clone", "design", "tts")

BASE_PROP = {"text": "Text", "language": "Language", "voice_id": "VoiceId",
             "voice_ref": "VoiceRef", "artifacts": "Artifacts",
             "audio_base64": "AudioBase64", "audio_path": "AudioPath"}
DESIGN_ROUTED = ("instruct", "instruction")
KEYWORDS = {"string", "object", "bool", "int", "float", "double"}
GATE_CAP = {"voice_id": "built_in_voices", "voice_ref": "speaker_reference"}
INTERFACE_DOC = {"clone": "Voice-clone convenience client contract for clone-capable model families.",
                 "design": "Voice-design convenience client contract for design-capable model families.",
                 "tts": "Text-to-speech convenience client contract for tts-capable model families."}
INTERFACE_METHOD = {"clone": "CloneAsync", "design": "DesignAsync", "tts": "SpeakAsync"}


def pascal(name):
    parts = name.split("_")
    if parts[0] in KEYWORDS:
        parts[0] = parts[0].capitalize()
    return "".join(p[:1].upper() + p[1:] for p in parts)


def member(value):
    parts = re.split("[-_]", str(value))
    name = "".join(p[0].upper() + p[1:] for p in parts if p)
    name = "".join(c for c in name if c.isalpha() or c.isdigit() or c == "_")
    if not name or not name[0].isalpha():
        name = "V" + name
    return name


def base_name(family, task):
    parts = family.split("_")
    if parts[-1] == task:
        parts = parts[:-1]
    return "".join(p[:1].upper() + p[1:] for p in parts)


def xml_escape(text):
    return str(text).replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def csharp_string(text):
    if text == "":
        return "string.Empty"
    return '"' + str(text).replace("\\", "\\\\").replace('"', '\\"') + '"'


def fmt_num(value):
    if isinstance(value, float):
        return str(int(value)) if value.is_integer() else repr(value)
    return str(value)


def kind_of(opt):
    if opt["name"] == "seed":
        return "string"
    return {"int": "long", "float": "double", "bool": "bool", "enum": "enum"}.get(opt["type"], "string")


def clean_enum_default(value, values):
    # A default outside the enum values (e.g. "") cannot be a member: treat as no default.
    return value if value in values else None


def load_specs():
    specs = {}
    for path in sorted(glob.glob(os.path.join(SPECS_DIR, "*.json"))):
        with open(path) as f:
            data = json.load(f)
        if "family" in data:
            specs[data["family"]] = data
    with open(os.path.join(SPECS_DIR, "model_params.json")) as f:
        params = json.load(f)
    return specs, params


def resolved_default(family, opt, params):
    entries = params.get(family)
    if isinstance(entries, list):
        for entry in entries:
            if entry.get("name") == opt["name"] and "default" in entry:
                return entry["default"]
    return opt.get("default")


def literal(kind, enum_name, value):
    if kind == "enum":
        return f"{enum_name}.{member(value)}" if value is not None else None
    if kind == "bool":
        return "true" if value else "false"
    if kind in ("long", "double"):
        return fmt_num(value) if isinstance(value, (int, float)) else None
    return csharp_string(value) if value is not None else None


def matches_spec_default(info, value):
    if info["spec_init"] is None:
        return False
    if info["kind"] == "bool":
        return bool(value) == bool(info["spec_default"])
    return value == info["spec_default"]


def option_info(family, task, opt, params, caps):
    kind = kind_of(opt)
    values = opt.get("values")
    if kind == "enum" and not values:
        kind = "string"
    default = resolved_default(family, opt, params)
    if kind == "enum":
        default = clean_enum_default(default, values)
    routed = task == "design" and opt["name"] in DESIGN_ROUTED
    enum_name = pascal(family) + pascal(opt["name"]) if kind == "enum" else None
    spec_default = clean_enum_default(opt.get("default"), values) if kind == "enum" else opt.get("default")
    init = literal(kind, enum_name, default)
    # The omission guard compares against the spec default: the server applies the spec
    # default, so a web-ui default that conflicts with it must travel on the wire.
    spec_init = literal(kind, enum_name, spec_default)
    return {
        "name": opt["name"],
        "prop": BASE_PROP.get(opt["name"], pascal(opt["name"])),
        "kind": kind,
        "type": enum_name if enum_name else kind,
        "init": init,
        "default": default,
        "spec_default": spec_default,
        "spec_init": spec_init,
        "values": values,
        "required": opt.get("required", False),
        "min": opt.get("min"),
        "max": opt.get("max"),
        "enum_name": enum_name,
        "routed": routed,
        "gated": opt["name"] in GATE_CAP and GATE_CAP[opt["name"]] not in caps.get(task, []),
        "description": opt.get("description", ""),
    }


def q(text):
    return csharp_string(text)


def membership_message(family, task, info):
    return f"{family} {task} {info['name']} must be one of: " + ", ".join(str(v) for v in info["values"])


def bounds_message(family, task, info):
    if info["min"] is not None and info["max"] is not None:
        return f"{family} {task} {info['name']} must be between {fmt_num(info['min'])} and {fmt_num(info['max'])}"
    if info["min"] is not None:
        return f"{family} {task} {info['name']} must be at least {fmt_num(info['min'])}"
    return f"{family} {task} {info['name']} must be at most {fmt_num(info['max'])}"


def gate_message(family, task, opt_name):
    return f"{family} {task} does not accept {opt_name} (capability {GATE_CAP[opt_name]} is false)"


def validate_body(family, task, infos):
    parts = []
    for opt_name in ("voice_id", "voice_ref"):
        info = next((i for i in infos if i["name"] == opt_name), None)
        if info and info["gated"]:
            parts.append(f"        if ({info['prop']} != null)\n        {{\n            throw new ArgumentException({q(gate_message(family, task, opt_name))});\n        }}")
    for info in infos:
        target = "Instruction" if info["routed"] else info["prop"]
        if info["required"]:
            parts.append(f"        if ({target} == null)\n        {{\n            throw new ArgumentException({q(f'{family} {task} requires {info['name']}')}, \"{target}\");\n        }}")
        if info["kind"] in ("long", "double") and (info["min"] is not None or info["max"] is not None):
            checks = []
            if info["min"] is not None:
                checks.append(f"{target}.Value < {fmt_num(info['min'])}")
            if info["max"] is not None:
                checks.append(f"{target}.Value > {fmt_num(info['max'])}")
            cond = f"{target} != null && (" + " || ".join(checks) + ")"
            parts.append(f"        if ({cond})\n        {{\n            throw new ArgumentException({q(bounds_message(family, task, info))}, \"{target}\");\n        }}")
    return "\n\n".join(parts)


def build_options_body(family, task, infos):
    parts = []
    for info in infos:
        target = "Instruction" if info["routed"] else info["prop"]
        cond = f"{target} != null"
        if info["spec_init"] is not None:
            cond += f" && {target} != {info['spec_init']}"
        if info["kind"] == "enum":
            arms = ",\n".join(f"            {info['enum_name']}.{member(v)} => {q(v)}" for v in info["values"])
            value = (f"{target} switch\n            {{\n{arms},\n"
                     f"            _ => throw new ArgumentException({q(membership_message(family, task, info))}),\n            }}")
        elif info["kind"] == "bool":
            value = f"{target}.Value ? \"true\" : \"false\""
        elif info["kind"] in ("long", "double"):
            value = f"{target}.Value.ToString(CultureInfo.InvariantCulture)"
        else:
            value = target
        parts.append(f"        if ({cond})\n        {{\n            options[\"{info['name']}\"] = {value};\n        }}")
    return "\n\n".join(parts)


def test_value(info):
    """A value that is set, != default and inside bounds (for the golden test)."""
    if info["kind"] == "enum":
        for value in info["values"]:
            if value != info["default"]:
                return value
        return info["values"][0]
    if info["kind"] == "bool":
        return not bool(info["default"])
    if info["kind"] == "long":
        base = info["default"] if isinstance(info["default"], int) else 0
        value = base + 1
        if info["min"] is not None and value < info["min"]:
            value = info["min"] + 1
        if info["max"] is not None and value > info["max"]:
            value = info["max"] - 1
        return value
    if info["kind"] == "double":
        base = info["default"] if isinstance(info["default"], (int, float)) else 0.0
        value = base + 1.0
        if info["min"] is not None and value < info["min"]:
            value = info["min"] + 0.5
        if info["max"] is not None and value > info["max"]:
            value = info["max"] - 0.5
        return value
    if info["kind"] == "string":
        return "x" if info["default"] != "x" else "y"
    return None


def required_value(info):
    """A valid value for a required option (for tests)."""
    if info["kind"] == "enum":
        return info["values"][0]
    if info["kind"] == "bool":
        return True
    if info["kind"] == "long":
        base = info["min"] if info["min"] is not None else (info["default"] if isinstance(info["default"], int) else 0)
        return base if base == 0 or info["max"] is None or base + 1 <= info["max"] else base
    if info["kind"] == "double":
        return info["min"] if info["min"] is not None else (info["default"] if isinstance(info["default"], (int, float)) else 0.0)
    return "x"


def wire_value(info, value):
    if info["kind"] == "bool":
        return "true" if value else "false"
    if info["kind"] in ("long", "double"):
        return fmt_num(value)
    return str(value)


def csharp_literal(info, value):
    if info["kind"] == "enum":
        return f"{info['enum_name']}.{member(value)}"
    if info["kind"] == "bool":
        return "true" if value else "false"
    if info["kind"] in ("long", "double"):
        return fmt_num(value)
    return csharp_string(value)


def model_default(family, spec):
    packages = spec.get("packages", [])
    for package in packages:
        if package.get("default"):
            return package["id"]
    if packages:
        return packages[0]["id"]
    ui = spec.get("ui", {})
    return ui.get("recommended_package") or family


WIRE_FIELD = {"text": "text", "language": "language", "audio": "audio_path",
             "audio_base64": "audio_base64", "voice_id": "voice_id",
             "voice_ref": "voice_ref", "artifacts": "artifacts"}


def golden_entry(family, task, infos, default_model):
    set_values = {}
    for info in infos:
        if info["required"]:
            set_values[info["name"]] = required_value(info)
    picked = None
    for info in infos:
        if info["name"] in set_values or info["gated"]:
            continue
        value = test_value(info)
        if value is not None and value != info["default"]:
            set_values[info["name"]] = value
            picked = info
            break
    if picked is None:
        return None
    by_name = {info["name"]: info for info in infos}
    # Effective property values as the C# object sees them: explicit in the initializer,
    # else the record initializer (gated and routed properties stay null).
    effective = {}
    for info in infos:
        if info["name"] in set_values:
            effective[info["name"]] = set_values[info["name"]]
        elif info["name"] == "text":
            effective[info["name"]] = "hello"
        elif not info["gated"] and not info["routed"] and info["init"] is not None:
            effective[info["name"]] = info["default"]
    options = {}
    for info in infos:
        if info["name"] in effective and not matches_spec_default(info, effective[info["name"]]):
            options[info["name"]] = wire_value(info, effective[info["name"]])
    request = {}
    for field, opt_name in WIRE_FIELD.items():
        info = by_name.get(opt_name)
        if info is None:
            if field == "text":
                request[field] = "hello"
            continue
        if opt_name in options:
            continue
        # BASE_PROP options are shadowed by `new` properties: the golden initializer
        # sets the derived property, the base wire field stays null, so only the
        # options map can carry these values.
    if options:
        request["options"] = options
    initializer = []
    if "text" not in set_values:
        initializer.append('Text = "hello"')
    for info in infos:
        if info["name"] in set_values:
            target = "Instruction" if info["routed"] else info["prop"]
            initializer.append(f"{target} = {csharp_literal(info, set_values[info['name']])}")
    return {
        "family": family,
        "name": base_name(family, task),
        "initializer": initializer,
        "json": json.dumps({"model": default_model, "request": request}, separators=(",", ":"), ensure_ascii=True),
        "uses_enum": any(by_name[n]["kind"] == "enum" for n in set_values),
    }


def validate_entry(family, task, infos):
    entries = []
    for info in infos:
        if info["gated"]:
            name = "Instruction" if info["routed"] else info["prop"]
            entries.append({
                "family": family,
                "name": base_name(family, task),
                "method": f"{base_name(family, task)}Validate{pascal(info['name'])}",
                "initializer": [f"{name} = {csharp_literal(info, required_value(info))}"],
                "message": gate_message(family, task, info["name"]),
                "param": None,
                "opt": info["name"],
                "uses_enum": info["kind"] == "enum",
                "gate": True,
            })
    target = None
    for info in infos:
        if info["kind"] not in ("long", "double") or (info["min"] is None and info["max"] is None):
            continue
        target = info
        break
    if target is not None:
        bad = target["min"] - 1 if target["min"] is not None else target["max"] + 1
        name = "Instruction" if target["routed"] else target["prop"]
        initializer = []
        for info in infos:
            if info["required"] and info is not target:
                rname = "Instruction" if info["routed"] else info["prop"]
                initializer.append(f"{rname} = {csharp_literal(info, required_value(info))}")
        initializer.append(f"{name} = {csharp_literal(target, bad)}")
        entries.append({
            "family": family,
            "name": base_name(family, task),
            "method": f"{base_name(family, task)}Validate",
            "initializer": initializer,
            "message": bounds_message(family, task, target),
            "param": name,
            "gate": False,
            "uses_enum": target["kind"] == "enum",
        })
    return entries


def prefix_entry(family, task, infos):
    if task != "design" or any(info["routed"] for info in infos):
        return None
    return {"family": family, "name": base_name(family, task)}


def test_file(task, entries, validate_entries, prefix_entries):
    groups = []
    if validate_entries:
        groups.append(["using System;"])
    mabs = []
    if any(e["uses_enum"] for e in entries) or any(e["uses_enum"] for e in validate_entries):
        mabs.append("using Mabs.AudioCpp.Enums;")
    mabs.append(f"using Mabs.AudioCpp.Models.{task.capitalize()};")
    mabs.append("using Mabs.AudioCpp.Models.Tasks;")
    mabs.append("using Mabs.AudioCpp.Utils;")
    groups.append(sorted(mabs))
    groups.append(["using Xunit;"])
    usings = "\n\n".join("\n".join(g) for g in groups)
    lines = [usings, "", "namespace Mabs.AudioCpp.Tests.Convenience;", "",
            f"/// <summary>Golden serialization, validation and composition tests for the generated {task} convenience records.</summary>",
            f"public class {task.capitalize()}GoldenTests", "{"]
    for entry in entries:
        lines.append(f"    /// <summary>Golden serialization for the generated <c>{entry['name']}{task.capitalize()}Request</c> (set and pre-initialized options emitted in spec order).</summary>")
        lines.append("    [Fact]")
        lines.append(f"    public void {entry['name']}Golden()")
        lines.append("    {")
        lines.append(f"        {entry['name']}{task.capitalize()}Request request = new() {{ {', '.join(entry['initializer'])} }};")
        model = json.loads(entry["json"])["model"]
        lines.append(f"        string json = Json.Serialize(new TaskRunRequest {{ Model = {q(model)}, Request = request.ToTaskRequestBody() }});")
        lines.append(f"        Assert.Equal({q(entry['json'])}, json);")
        lines.append("    }")
        lines.append("")
    for entry in validate_entries:
        if entry["gate"]:
            lines.append(f"    /// <summary>A set <c>{entry['opt']}</c> fails the capability gate for the generated <c>{entry['name']}{task.capitalize()}Request</c>.</summary>")
        else:
            lines.append(f"    /// <summary>An out-of-range value fails validation for the generated <c>{entry['name']}{task.capitalize()}Request</c>.</summary>")
        lines.append("    [Fact]")
        lines.append(f"    public void {entry['method']}()")
        lines.append("    {")
        lines.append(f"        {entry['name']}{task.capitalize()}Request request = new() {{ {', '.join(entry['initializer'])} }};")
        lines.append("        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());")
        lines.append(f"        Assert.Contains({q(entry['message'])}, ex.Message);")
        if entry["param"] is not None:
            lines.append(f"        Assert.Equal({q(entry['param'])}, ex.ParamName);")
        lines.append("    }")
        lines.append("")
    for entry in prefix_entries:
        lines.append(f"    /// <summary>The generated <c>{entry['name']}{task.capitalize()}Request</c> has no instruction option, so the instruction prefixes the text.</summary>")
        lines.append("    [Fact]")
        lines.append(f"    public void {entry['name']}Prefix()")
        lines.append("    {")
        lines.append(f"        {entry['name']}{task.capitalize()}Request request = new() {{ Text = \"hello\", Instruction = \"warm\" }};")
        lines.append("        TaskRequestBody body = request.ToTaskRequestBody();")
        lines.append("        Assert.Equal(\"(warm) hello\", body.Text);")
        lines.append("        Assert.Null(body.Options);")
        lines.append("    }")
        lines.append("")
    if lines and lines[-1] == "":
        lines.pop()
    lines.append("}")
    return "\n".join(lines)


def enum_file(name, family, opt_name, values):
    lines = ["namespace Mabs.AudioCpp.Enums;", "",
            f"/// <summary>Wire values for the <c>{family}</c> <c>{opt_name}</c> option.</summary>",
            f"public enum {name}", "{"]
    members = [f"    /// <summary>Wire value <c>{xml_escape(v)}</c>.</summary>\n    {member(v)}," for v in values]
    body = "\n\n".join(members)
    return "\n".join(lines) + "\n" + body + "\n}"


def record_file(family, task, infos, default_model):
    class_name = base_name(family, task) + task.capitalize() + "Request"
    kinds = {info["kind"] for info in infos}
    needs_system = any(info["gated"] or info["required"] or info["kind"] == "enum"
                       or (info["kind"] in ("long", "double") and (info["min"] is not None or info["max"] is not None))
                       for info in infos)
    sys_group = []
    if needs_system:
        sys_group.append("using System;")
    sys_group.append("using System.Collections.Generic;")
    if "long" in kinds or "double" in kinds:
        sys_group.append("using System.Globalization;")
    mabs_group = ["using Mabs.AudioCpp.Enums;"] if "enum" in kinds else []
    usings = "\n\n".join(filter(None, ["\n".join(sys_group), "\n".join(mabs_group)]))
    lines = [usings, "", f"namespace Mabs.AudioCpp.Models.{task.capitalize()};", "",
            f"/// <summary>Convenience request for the <c>{family}</c> spec, {task} task (model default <c>{default_model}</c>).</summary>",
            f"public sealed record {class_name} : {task.capitalize()}Request", "{"]
    props = []
    for info in infos:
        if info["routed"]:
            continue
        shadow = "new " if info["name"] in BASE_PROP else ""
        summary = xml_escape(info["description"]) if info["description"] else f"Option <c>{info['name']}</c>."
        # Gated options get no initializer: Validate() rejects any set value, so a
        # pre-initialized default would make every request throw.
        init_part = f" = {info['init']};" if info["init"] is not None and not info["gated"] else ""
        props.append(f"    /// <summary>{summary}</summary>\n    public {shadow}{info['type']}? {info['prop']} {{ get; init; }}{init_part}")
    body = ["\n\n".join(props)]
    if task == "design" and not any(info["routed"] for info in infos):
        body.append("    /// <inheritdoc />\n    public override string? ComposedText()\n    {\n        return Instruction == null ? Text : $\"({Instruction}) {Text}\";\n    }")
    body.append("    /// <inheritdoc />\n    public override void Validate()\n    {\n        base.Validate();\n\n" + validate_body(family, task, infos) + "\n    }")
    body.append("    /// <inheritdoc />\n    public override Dictionary<string, string> BuildOptions()\n    {\n        Dictionary<string, string> options = [];\n\n" + build_options_body(family, task, infos) + "\n\n        return options;\n    }")
    return class_name, "\n".join(lines) + "\n" + "\n\n".join(body) + "\n}"


def client_file(family, task, default_model):
    class_name = base_name(family, task) + task.capitalize() + "Client"
    mabs = sorted(["Mabs.AudioCpp.Interfaces",
                   f"Mabs.AudioCpp.Models.{task.capitalize()}", "Mabs.AudioCpp.Models.Tasks"])
    usings = "\n\n".join([
        "\n".join(["using System.Collections.Generic;", "using System.Threading;", "using System.Threading.Tasks;"]),
        "\n".join("using " + m + ";" for m in mabs)])
    lines = [usings, "",
             f"namespace Mabs.AudioCpp.{task.capitalize()};", "",
             f"/// <summary>Convenience {task} client for the <c>{family}</c> family (model default <c>{default_model}</c>).",
             f"/// The configured model id must serve the <c>{task}</c> task: <c>/v1/tasks/run</c> carries no task",
             "/// field, the server resolves it from the model entry.</summary>",
             f"public sealed class {class_name} : I{task.capitalize()}Client",
            "{",
            "    private readonly AudioCppClient _client;",
            "",
            "    /// <summary>Initializes the client bound to an existing <see cref=\"AudioCppClient\"/>.</summary>",
            "    /// <param name=\"client\">The underlying client.</param>",
            f"    public {class_name}(AudioCppClient client)",
            "    {",
            "        _client = client;",
            "    }",
            "",
            f"    /// <summary>Model id to run against; must serve the <c>{task}</c> task. Defaults to the spec",
            f"    /// default package <c>{default_model}</c>.</summary>",
            f"    public string Model {{ get; set; }} = {q(default_model)};",
            "",
            f"    /// <summary>Runs one {task} request against <see cref=\"Model\"/>.</summary>",
            "    /// <param name=\"request\">The request (validated before sending).</param>",
            "    /// <param name=\"cancellationToken\">Cancellation token observed while enumerating.</param>",
            "    /// <returns>The task result.</returns>",
            f"    public Task<TaskResult> {INTERFACE_METHOD[task]}({task.capitalize()}Request request, CancellationToken cancellationToken = default)",
            "    {",
            f"        return _client.Tasks.RunAsync(new TaskRunRequest {{ Model = Model, Request = request.ToTaskRequestBody() }}, cancellationToken);",
            "    }",
            "",
            f"    /// <summary>Streams one {task} request against <see cref=\"Model\"/>.</summary>",
            "    /// <param name=\"request\">The request (validated before sending).</param>",
            "    /// <param name=\"cancellationToken\">Cancellation token observed while enumerating.</param>",
            "    /// <returns>The stream events.</returns>",
            f"    public IAsyncEnumerable<TaskStreamEvent> StreamAsync({task.capitalize()}Request request, CancellationToken cancellationToken = default)",
            "    {",
            f"        return _client.Tasks.StreamAsync(new TaskStreamRequest {{ Model = Model, Request = request.ToTaskRequestBody() }}, cancellationToken);",
            "    }",
            "}"]
    return "\n".join(lines)


def interface_file(task):
    mabs = sorted([f"Mabs.AudioCpp.Models.{task.capitalize()}", "Mabs.AudioCpp.Models.Tasks"])
    lines = ["using System.Threading;", "using System.Threading.Tasks;", "",
             "\n".join("using " + m + ";" for m in mabs), "",
             "namespace Mabs.AudioCpp.Interfaces;", "",
             f"/// <summary>{INTERFACE_DOC[task]}</summary>",
             f"public interface I{task.capitalize()}Client", "{",
             f"    /// <summary>Runs one {task} request against the configured model.</summary>",
             "    /// <param name=\"request\">The request (validated before sending).</param>",
             "    /// <param name=\"cancellationToken\">Cancellation token.</param>",
             "    /// <returns>The task result.</returns>",
             f"    Task<TaskResult> {INTERFACE_METHOD[task]}({task.capitalize()}Request request, CancellationToken cancellationToken = default);",
             "}"]
    return "\n".join(lines)


def main():
    specs, params = load_specs()
    out_records = os.path.join(ROOT, "src", "AudioCpp", "Models")
    out_enums = os.path.join(ROOT, "src", "AudioCpp", "Enums")
    out_interfaces = os.path.join(ROOT, "src", "AudioCpp", "Interfaces")
    out_clients = os.path.join(ROOT, "src", "AudioCpp")
    out_tests = os.path.join(ROOT, "tests", "AudioCpp.Tests", "Convenience")
    for task in TASKS:
        os.makedirs(os.path.join(out_records, task.capitalize()), exist_ok=True)
        os.makedirs(os.path.join(out_clients, task.capitalize()), exist_ok=True)
    os.makedirs(out_enums, exist_ok=True)
    os.makedirs(out_interfaces, exist_ok=True)
    os.makedirs(out_tests, exist_ok=True)

    pairs = [(family, task, spec) for family, spec in sorted(specs.items())
            for task in sorted(spec["tasks"]) if task in TASKS]

    # Enums: one file per enum, first occurrence wins (same option name shares its enum across tasks).
    enums = {}
    for family, spec in sorted(specs.items()):
        for opt in spec["options"].get("request", []):
            if kind_of(opt) == "enum" and opt.get("values"):
                name = pascal(family) + pascal(opt["name"])
                if name in enums:
                    assert enums[name][2] == opt["values"], f"enum name collision {name}"
                else:
                    enums[name] = (family, opt["name"], opt["values"])

    test_entries = {task: [] for task in TASKS}
    validate_entries = {task: [] for task in TASKS}
    prefix_entries = {task: [] for task in TASKS}
    for family, task, spec in pairs:
        infos = [option_info(family, task, opt, params, spec.get("capabilities", {})) for opt in spec["options"].get("request", [])]
        default_model = model_default(family, spec)
        class_name, content = record_file(family, task, infos, default_model)
        with open(os.path.join(out_records, task.capitalize(), class_name + ".cs"), "w") as f:
            f.write(content)
        with open(os.path.join(out_clients, task.capitalize(), base_name(family, task) + task.capitalize() + "Client.cs"), "w") as f:
            f.write(client_file(family, task, default_model))
        g = golden_entry(family, task, infos, default_model)
        if g:
            test_entries[task].append(g)
        v = validate_entry(family, task, infos)
        validate_entries[task].extend(v)
        p = prefix_entry(family, task, infos)
        if p:
            prefix_entries[task].append(p)

    for name, (family, opt_name, values) in enums.items():
        with open(os.path.join(out_enums, name + ".cs"), "w") as f:
            f.write(enum_file(name, family, opt_name, values))

    for task in TASKS:
        with open(os.path.join(out_interfaces, f"I{task.capitalize()}Client.cs"), "w") as f:
            f.write(interface_file(task))
        with open(os.path.join(out_tests, f"{task.capitalize()}GoldenTests.cs"), "w") as f:
            f.write(test_file(task, test_entries[task], validate_entries[task], prefix_entries[task]))

    print(f"{len(pairs)} pairs, {len(enums)} enums, "
          + ", ".join(f"{task}: {len(test_entries[task])} golden + {len(validate_entries[task])} validate" for task in TASKS))


if __name__ == "__main__":
    main()