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
| **What-if** | **Start / End What-if Scenario** / **Recalculate** / **Reset** | The in-memory sandbox — see §3. |

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
2. **Start What-if Scenario** — the *What-if Editor* panel opens on the right with one row per
   editable parameter value:
   - **Element** — the element's own short-name first, then its containment path in brackets,
     e.g. `BUS  (…SpaceSeg.SV.DrySV)`, so elements sharing a prefix can be told apart.
   - **Parameter** — the parameter (column) name; state-dependent values append the state, e.g.
     `Power_budget [GC]`.
   - **Current** — the model value; a trailing **`*`** means it is a per-usage *override*
     (otherwise it's a shared element-definition value used by all usages).
   - **What-if** — type your new value here.
3. **Recalculate** — the collector re-runs against an in-memory copy of the model with your edits
   applied, and the preview refreshes. Nothing is saved.
4. **Reset** — discards all what-if edits and restores the true model values in both columns
   (panel stays open).
5. **End What-if Scenario** (the same toggle button) — reverts every edit, restores the original
   state and closes the panel.
6. To keep a change for real, use **Submit → Parameter Values**. While a scenario is active its edits
   are in the model, so Submit **warns first** and lets you cancel — use *End* / *Reset* if you meant
   to submit only the original values.

> **Values surfaced through custom report parameters.** Some reports compute a headline figure from
> a specific element rather than the detail rows — e.g. a power report's *Bus Power Available* /
> *Remaining Power* read a **BUS** element's (state-dependent) `Power_budget` via report parameters
> (`?dyn_…`). These update on Recalculate, but only if you edit **that** element's value (find the row
> whose *Element* ends in `.BUS`, for the right state), not a similarly-named parameter on another
> element.

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
- Only values that actually drive the report are editable: **leaf elements** (equipment) are always
  shown — including **unvalued** ones (blank *Current*), so a missing value can be filled in — while
  an **aggregating (non-leaf) element** is shown only when it *holds its own value* (e.g. a margin
  defined at subsystem level). A parent's rolled-up total is a report summary, not a stored value, so
  it is deliberately not offered for editing.
- **State-dependent** values are labelled with their state in the *Parameter* column
  (e.g. `Mass [hot]`) so the otherwise-identical rows can be told apart.

### Limits (current)
- The editable set is broader than the report's own detail rows in that it includes any element that
  owns a value (not just those the report happens to display).
- To what-if a **different option**, select it and Rebuild Datasource (or reopen); that re-pins it.
- Non-scalar (vector/compound) parameters aren't editable.
- What-if scenarios are **in-memory only** — they're discarded when the report/panel closes.
