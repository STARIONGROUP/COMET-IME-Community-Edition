# CDP4Reporting — Report Designer & What-if Editor

The **Reporting** plugin embeds the DevExpress Report Designer in the IME. You author a report
against the open iteration by writing a small **C# data collector** (the *Datasource* script),
laying out bands/fields in the designer, and previewing the result. From the preview you can
**submit** edited parameter values back to the model, or explore changes safely with the
**What-if Editor**.

Open it from the ribbon: **Reporting** → open the *Report Designer* panel for the active iteration.

---

## 1. Ribbon commands

### File group
| Command | What it does |
|---|---|
| **New Report** | Start an empty report. |
| **Open Report** | Open a saved report archive (`.rep4` / `.repx` bundle: layout + Datasource script). |
| **Save Report** / **Save Report As** | Save the current report (layout + script) to a report archive. |

### Data group
| Command | What it does |
|---|---|
| **Import Script** | Load a `Datasource.cs` collector script into the report. |
| **Export Script** | Save the current Datasource script to a `.cs` file. |
| **Compile** | Compile the Datasource script; errors appear in the **Errors** panel. |
| **Toggle Auto Compile** | Recompile automatically as you edit the script. |
| **Rebuild Datasource** | Re-run the collector against the model to refresh the report's data table. |

### Preview tab
| Group | Command | What it does |
|---|---|---|
| **Submit** | **Parameter Values** | Write the values shown in the preview back to the model (a real, saved change). |
| **Rebuild** | **Rebuild Datasource** | Re-run the collector and refresh the preview. |
| **What-if** | **Load Values** / **Recalculate** / **Reset** | The in-memory sandbox — see §3. |

Standard DevExpress designer features (field list, band editing, grouping, summaries, and
**Export to PDF/XLSX/…**) are available on the built-in designer surface as usual.

### Panels
- **Output** — collector/what-if progress messages (Clear to empty it).
- **Errors** — compile errors from the Datasource script.
- **What-if Editor** — the editable value grid (appears when you click *Load Values*).

---

## 2. Writing a report (Datasource script, in brief)

A report's data comes from a collector row type deriving from `DataCollectorRow`. Each parameter
you want as a column is declared with `[DefinedThingShortName("<modelShortName>", "<Column>")]`:

```csharp
[DefinedThingShortName("m", "Mass")]
public DataCollectorDoubleParameter<Row> Mass { get; set; }

[DefinedThingShortName("mass_margin", "MassMargin")]
public DataCollectorDoubleParameter<Row> MassMargin { get; set; }
```

`[CollectParentValues]` collects a value from parent elements; derived columns (e.g.
`TotalMass = Mass * (1 + MassMargin/100)`) are plain C# properties. Compile, lay out the bands,
and preview.

> The reporting **engine** (collectors, `DataCollectorRow`, parameter types, roll-up utilities)
> lives in the `CDP4Reporting-CE` SDK package — this plugin is the designer UI and the
> submit / what-if glue around it.

---

## 3. What-if Editor (non-saving sandbox)

Lets a stakeholder change leaf parameter values and see every roll-up recompute — **without
saving anything**. Nothing touches the model or the server until you explicitly **Submit**.

### Use it
1. Open a report and go to the **Preview** tab (make sure it has data — Rebuild if needed).
2. **Load Values** — the *What-if Editor* panel opens on the right with one row per editable
   parameter value:
   - **Element** — the element the value belongs to.
   - **Parameter** — the parameter (column) name.
   - **Current** — the model value; a trailing **`*`** means it is a per-usage *override*
     (otherwise it's a shared element-definition value used by all usages).
   - **What-if** — type your new value here.
3. **Recalculate** — the collector re-runs against an in-memory copy of the model with your edits
   applied, and the preview refreshes. Nothing is saved.
4. **Reset** (or **Load Values** again) — discards all what-if edits and restores the true model
   values in both columns.
5. To keep a change for real, use **Submit → Parameter Values**.

### How it decides what's editable
- Only parameters the report declares with `[DefinedThingShortName]` are offered — so **derived /
  computed columns are never editable** (you edit the inputs; the totals recompute).
- Values are read straight from the model's nested-parameter tree, so **no `ModelPath` column or
  extra report setup is required** — any report with `[DefinedThingShortName]` params lights up.
- The grid is scoped to a **single option** — the report's selected option, **pinned** when the
  report is opened or its datasource is rebuilt and kept until the next open/rebuild (shown in the
  Output line). It does not change between Load and Recalculate.
- Values **shared** across usages (one element-definition value) collapse to a **single row** and
  are kept in sync; per-usage **overrides** are shown separately and marked `*`.
- A row is shown for **every value the element actually owns**, including parameters that are
  declared but currently **unvalued** (blank *Current*, still editable).
- **State-dependent** values are labelled with their state in the *Parameter* column
  (e.g. `Mass [hot]`) so the otherwise-identical rows can be told apart.

### Limits (current)
- Shows every element in the option's tree that owns a matching parameter — this can be **more rows
  than the report's own detail rows**; it is not filtered to the report's row set.
- To what-if a **different option**, select it and Rebuild Datasource (or reopen); that re-pins it.
- Non-scalar (vector/compound) parameters aren't editable.
- What-if scenarios are **in-memory only** — they're discarded when the report/panel closes.
