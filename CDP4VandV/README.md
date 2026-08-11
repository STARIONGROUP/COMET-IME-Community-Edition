# CDP4VandV: Verification and Validation

A CDP4-COMET IME plugin that turns an engineering model into a working **Verification Control Document (VCD)**:
who verifies which requirement, how, at which stage gate, whether it passed, whether the design actually complies,
and whether the customer has accepted it.

It adds **no SDK types and no metamodel change**. Everything is expressed in stock ECSS-E-TM-10-25A concepts, so a
model produced here opens in any other COMET client without that client knowing this plugin exists.

---

## 1. Concepts

### 1.1 What a V&V item is

A **V&V item** is one verification activity: "verify REQ-1 by inspection at CDR". In the model it is a plain
`Requirement`, distinguished only by being categorized **`VnVItem`**:

| Concept | Model representation |
|---|---|
| V&V item | `Requirement` categorized `VnVItem` |
| Where it lives | a `RequirementsSpecification` with short-name **`VNV`**, created on first use |
| Its attributes | `SimpleParameterValue`s on that requirement, keyed by parameter type short-name (`vnv_method`, `vnv_stage`, ...) |
| What it verifies | a `BinaryRelationship` categorized **`verifies`** or **`validates`**, from the item to the requirement |
| What it measures | `BinaryRelationship`s categorized `coversParameter`, `verifiedOn`, `coversOption`, `coversState` |
| Its procedure | one `Requirement` per step, categorized `VnVStep`, linked by a `hasStep` `BinaryRelationship` |
| Non-conformances | `ReviewItemDiscrepancy`, `RequestForDeviation`, `RequestForWaiver` on the `EngineeringModel` |

Because a V&V item is itself a requirement, one requirement can be covered by many items (one per method, level or
stage gate combination), which is exactly what ECSS-E-ST-10-02 expects of a verification strategy.

### 1.2 Three statuses that are deliberately not the same thing

This trips people up, so the plugin keeps them apart everywhere, including in the export:

| Question | Field | Example |
|---|---|---|
| Did the activity happen, and what did it show? | **Execution status** `vnv_status` | `Executed`, `Passed`, `Failed` |
| Does the design meet the requirement? | **Compliance** `vnv_compliance` | `Compliant`, `Partially Compliant` |
| Has the customer accepted it, and why? | **Close-out** `vnv_closed` + reason | `Closed`, "accepted at CDR under RFW-004" |

A test can pass while the requirement is only partly met. That is precisely the case a waiver exists for, and a tool
that collapses these three into one column cannot represent it.

---

## 2. Getting started

### 2.1 Set up the reference data (once per model)

The plugin needs parameter types, categories and rules in the model's RDL chain. Nothing is assumed to be there.

1. **Model menu > Set up V&V**.
2. It reports exactly what is missing and what it will create.
3. Confirm. It writes only what is absent, so it is safe to run again after an upgrade.

The check walks the whole chained RDL (`QueryParameterTypesFromChainOfRdls` and friends), so anything already
defined in a site RDL is reused rather than duplicated. If you lack write access to the RDL, the dialog says so and
lists what an administrator must create.

> **After upgrading the plugin, run Set up V&V again.** New releases add parameter types. Items created before an
> upgrade keep working; the new columns simply read empty until they are filled in.

### 2.2 Open the register

**Requirements ribbon > V&V Register**. It docks in the middle document area, next to your model browsers.

The tree mirrors the stock Requirements browser (specification > group > requirement) and nests the V&V items under
the requirement each one covers, so the two browsers read alike.

---

## 3. Daily use

### 3.1 Create a V&V item

Right-click a requirement > **Create V&V Item for this Requirement**. One dialog, six tabs:

| Tab | Holds |
|---|---|
| **Basic** | link type, short name, name, owner, method, stage gate, acceptance criteria, description |
| **Planning** | method, stage gate, acceptance criteria (kept in sync with Basic), level, criticality, plan reference, activity number, planned date, external party, coverage note |
| **Procedure** | procedure reference, preconditions, conditions, facility, and the ordered steps |
| **Execution** | status, actual date, result, evidence reference |
| **Compliance** | compliance status, the read-only analysis check, and the close-out record |
| **Coverage** | which parameter, element, options and states this activity actually measures |

On OK the plugin creates, in **one transaction**: the item, all its attributes, the `verifies` (or `validates`)
relationship, the `VNV` specification if it did not exist, and any coverage links.

**Link type** decides the verb. `verifies` checks the item against the requirement baseline; `validates` checks it
against the mission or stakeholder need. Changing it rewrites the suggested name, unless you have typed your own.

**Acceptance criteria from a constraint.** If the requirement carries `ParametricConstraint`s, a *from constraint*
picker appears under the criteria box. Pick a whole constraint or a single relational expression, press **Use**, and
its expression text is appended to the criteria. If that expression is bound to a parameter, the coverage link is
set at the same time, which is what later enables the analysis check.

### 3.2 Record coverage

Coverage narrows an activity to what it actually measures. Two ways:

- **Coverage tab**: pick the element definition, the parameter, and (only if that parameter is option or state
  dependent) the options and states.
- **Drag and drop**: drag a parameter from the Element Definitions browser onto a V&V item row. A plain parameter is
  coupled in one gesture. An option or state dependent parameter opens the dialog on the Coverage tab with the
  parameter preselected and nothing ticked, because the drag payload cannot say which slice you meant.

The dialog's selection is **authoritative**: clearing it deletes the existing coverage links.

> Dropping a parameter replaces that item's coverage wholesale. There is no merge and no undo.

### 3.2a Write the procedure

The **Procedure** tab is the verification procedure or test specification: what an operator actually does, and what
was actually observed. ECSS-E-ST-10-03 expects a test procedure of numbered steps, and a test report to carry the
*as-run* procedure with the outcome of each step, which a single free-text field cannot record.

The header fields describe the procedure as a whole: **Procedure Ref.** when the procedure is written down in a
separate document, plus **Preconditions**, **Conditions** and **Facility**.

The grid below holds the steps, each with:

| Column | Filled in |
|---|---|
| **#** | automatically, from the row order |
| **Action** | while planning: what the operator does |
| **Expected** | while planning: what should be observed if the step passes |
| **Actual** | while running: what was actually observed, the as-run record |
| **Result** | while running: `Not Run`, `Pass`, `Fail`, `Blocked` or `Not Applicable` |

**Add**, **Remove**, **Up** and **Down** manage the list. Steps are renumbered from their position, so reordering
never leaves a gap. Nothing is written until you press OK, so you can rearrange freely and cancel out.

Each step is stored as its own `Requirement` categorized `VnVStep`, in the same `VNV` specification as the item,
linked to it by a `hasStep` relationship. That is exactly how a V&V item itself is encoded, so steps are queryable,
exportable and visible to any other COMET client without a metamodel change.

> Removing a step **deprecates** it rather than deleting it. A `Requirement` is deprecatable and the SDK refuses to
> hard delete one, which is also why the stock browsers offer Deprecate rather than Delete. The step disappears from
> the procedure either way; the record of it having existed does not.

The completeness rule cross-checks the procedure against the outcome: an item that reports `Passed` or `Compliant`
while one of its steps is marked `Fail` is flagged, naming the step numbers that failed.

### 3.2b Choose a view and a stage gate

The register carries more columns than anyone needs at once, so the toolbar has two pickers.

**View** switches the column set to the job in hand:

| View | Shows |
|---|---|
| **Register (VCD)** | everything, the full Verification Control Document |
| **Planning** | method, stage gate, level, criticality, owner, activity number, planned date, acceptance criteria |
| **Procedure** | the procedure summary, plus each step expanded as a row with its action, expected result, as-run result and outcome |
| **Execution** | status, actual date, result, evidence |
| **Compliance** | compliance, close-out, analysis check |

**Stage** narrows the tree to the activities planned at one stage gate, so a CDR review shows only what is due at
CDR. The list comes from the model's own stage gates, so a project that defines its own can filter on them. A
requirement whose activities are all filtered out drops out with them, so the roll-up never reports a gap the model
does not have.

These are views of one tree rather than separate panels, which means the selection, the expansion state and every
context-menu action keep working identically in all of them. The tree also keeps whatever you had expanded when
rows refresh after an edit.

### 3.3 Read the tree

| Column | Means |
|---|---|
| **Coverage** on a requirement | `2 item(s): 1 passed, 1 open`, or `Not covered` |
| **Coverage** on a group or specification | `7/12 verified, 1 failed` |
| **Compliance** | the compliance status of that item |
| **Close-out** | `Open`, or `Closed: <reason>` |
| **Procedure** | `0 of 3 step(s) recorded`, plus the failed count. "Recorded" means somebody set that step's Result to something other than `Not Run`; nothing is executed by the tool |
| **Analysis Check** | the automatic verdict, see 3.5 |

In the Register and Procedure views a V&V item expands to its procedure steps, each with its own row and glyph.
Selecting a step and pressing Ctrl+E opens its item on the Procedure tab, where steps are edited.

A requirement counts as verified only when **every** item on it has closed out, so one open item keeps it out of the
numerator. Close-out wins over execution status: an explicitly closed item counts as done whatever its status says.
`Passed`, `Waived`, `Deviated` and `Not Applicable` need no further action; everything else is open.

Icons distinguish the three review states of an item: a plain checklist (no requests raised), a warning glyph (at
least one open request), a tick (all requests closed out).

### 3.4 Raise and work a non-conformance

Right-click anything > **Create a ...**. Five kinds are offered, all written by this plugin because the IME
registers no dialog for any of them:

| Kind | Use it for |
|---|---|
| **Review Item Discrepancy** | a problem found at a review, the ECSS non-conformance mechanism |
| **Request for Deviation** | approval to depart from a requirement **before** the product is built |
| **Request for Waiver** | approval to accept a product that **already** departs from a requirement |
| **Change Request** | a proposed change |
| **Model Note** | a free-form note, no approval workflow |

The first three are *review requests*: they drive the item icons, the NCR sheet and the close-out rule. A change
request and a model note are annotations but not concessions against a requirement, so they are excluded from those.

Once raised, **Check Annotations** lists every request on the selected row with its actions:

- **Open...** the floating annotation window
- **Reply...** post a discussion item
- **Mark Implemented** set status `DONE`
- **Close** set status `CLOSED`
- **Reopen** set status `OPEN`

Transitions grey out according to the current status, so you cannot close something already closed.

### 3.5 Analysis check (verification by analysis, from the model)

**Run Analysis Check** on the toolbar or in the context menu. For every V&V item it:

1. follows the item's `coversParameter` link,
2. finds the verified requirement's parametric constraints on that parameter type,
3. evaluates the parameter's **current actual value** against them, honouring the option and state slice the item
   covers,
4. writes the verdict into the Analysis Check column and the VCD export.

```
Meets constraint: m, 1 check(s) passed
VIOLATED: m [Option2, State1] is 28.4, required <= 25
Not checked: no covered parameter
```

Every state says something. A blank cell would read as "checked and fine", which is exactly wrong for an item that
was never checked at all.

It re-runs automatically whenever a `ParameterValueSet` changes, so moving a design value updates the register
immediately, and it also shows live on the Compliance tab while you are still filling the item in.

**It never writes a status.** Deciding that a requirement is met stays a human act; the check informs that judgement
and colours the row, nothing more.

#### How this differs from the Requirements browser's *Verify Requirements*

They are the same family but answer different questions:

| | Verify Requirements (stock) | Run Analysis Check (this plugin) |
|---|---|---|
| Evaluates against | the whole product tree from `TopElement` | the one parameter the item declares it covers |
| Needs | `TopElement` set, and you pick an Option | only the `coversParameter` link |
| Slice | per Option | the exact option and state slice the item covers |
| Result | transient Pass/Fail/Inconclusive, not persisted | per-item text in the register and the export |
| Runs | manually | on every value change, plus manually |

They can legitimately disagree. Theirs asks "does the design as a whole satisfy this requirement in option X"; this
one asks "does the specific quantity this activity measures meet its limit".

### 3.6 Close out

On the Compliance tab: set **Compliance**, tick **Closed**, and state the **Reason** (mandatory, the dialog refuses
to save a closed item without one), plus who closed it and when.

The completeness rule then enforces the ECSS discipline:

- closed with no reason
- closed as `Partially Compliant` or `Non-Compliant` with **no closed waiver or deviation** conceding the shortfall
- closed while a review request against it is still open

### 3.7 Coverage matrix (RVM)

**Coverage Matrix** on the toolbar opens a live requirements-by-stage-gate matrix. Cells name the item and its
state, for example `VNV-1: Inspection (Planned)`. The header line counts covered against uncovered requirements.

Stage gates come from the RDL, not from a hard-coded list, plus any stage actually in use in the model. Define your
own stage gates in the RDL and they become columns with no rebuild.

### 3.8 Export

**Export VCD / RVM** writes one workbook with four sheets:

| Sheet | Contents |
|---|---|
| **VCD** | one row per activity, 27 columns, every field ECSS-E-ST-10-02 Annex B lists, including requirement text, parent requirement, compliance, close-out and the analysis check. Uncovered requirements appear in red. |
| **RVM** | requirements down, stage gates across, plus a Covered column |
| **Execution Records** | only activities that have actually been executed |
| **Procedures** | one row per procedure step, with the item, the requirement, the procedure header, and each step's action, expected result, as-run result and outcome. Failed steps appear in red. |
| **NCRs** | every review request: type, id, open flag, status, V&V item, requirement, title, classification, owner, created, content, reply and solution counts |

### 3.9 Built-in rules

Both run from the stock **Built-In Rules** browser, alongside the model's other rules:

- **RequirementVnVCoverage**: requirements no V&V item verifies or validates.
- **VnVItemCompleteness**: items unfit to execute or report, that is no traceability link, no method, no stage gate,
  no acceptance criteria, a concluded status with no result, or the close-out defects in 3.6.

Only links whose **source is a V&V item** count as coverage. A `verifies` link authored between two ordinary
requirements is requirement traceability, not verification coverage.

---

## 4. Reference data

`Rdl/VandVRdlManifest.cs` is the single declaration of everything the plugin needs. Adding an attribute is one list
entry: the checker and the seeder are plain functions over these lists.

### 4.1 Parameter types

**Planning**: `vnv_method`, `vnv_stage`, `vnv_level`, `vnv_criticality`, `vnv_activity_no`, `vnv_description`,
`vnv_preconditions`, `vnv_conditions`, `vnv_acceptance`, `vnv_facility`, `vnv_responsible_ext`, `vnv_planned_date`,
`vnv_coverage_note`, `vnv_plan_ref`

**Procedure**: `vnv_procedure_ref`, and per step `vnv_step_no`, `vnv_step_action`, `vnv_step_expected`,
`vnv_step_actual`, `vnv_step_result`

**Execution**: `vnv_status`, `vnv_actual_date`, `vnv_result`, `vnv_evidence_ref`

**Close-out**: `vnv_compliance`, `vnv_closed`, `vnv_closeout_reason`, `vnv_closed_by`, `vnv_closed_on`

Every enumeration is read from the RDL at runtime, so a project can add its own methods, stage gates or levels
without touching code.

### 4.2 Categories

`VnVItem` (plus `VerificationItem` and `ValidationItem` as sub-categories), `VnVStep`, `TestCampaign`,
`StageGateGroup`, `NCR`, and the relationship categories `verifies`, `validates`, `coversOption`, `coversState`,
`coversParameter`, `verifiedOn`, `hasStep`.

The requirement category is auto-detected: the plugin matches `REQUIREMENT` or `REQ` case-insensitively, including
sub-categories that carry one of those as a super-category.

---

## 5. Architecture

```
Rdl/           manifest, check-and-seed service, check result
Services/      coverage query, roll-up, close-out vocabulary, analysis checker,
               item creator, coverage writer, annotation kinds/query/creator, workbook exporter
Rules/         the two IBuiltInRules
ViewModels/    browser, matrix, item dialog, annotation dialog, ribbon, rows
Views/         XAML, plus the tree node image selector in Selectors/
```

Points worth knowing before changing anything:

- **`VandVCoverageQuery` is the single source of truth** for what counts as coverage and how status rolls up. The
  browser, the matrix, the rules and the exporter all go through it, so they cannot disagree.
- **Enum values have two spellings.** The stock parameter-value editor stores a value definition's *shortName*
  (`Not_Applicable`), this plugin's dialog stores its *name* (`Not Applicable`). `AreSameEnumValue` normalises both;
  use it rather than `==` on any enumeration attribute.
- **Deleting inside a transaction** must also remove the thing from the registered clone's containment list, or the
  update DTO ships a dangling reference and the whole write is rejected.
- **The base browser populates the context menu before this class's fields are assigned.** Anything the menu reads
  must tolerate being null on that first pass.
- **The IME registers no `ThingDialog` for any annotation kind**, which is why `AnnotationCreator` writes them
  directly. The stock create-annotation commands throw "not registered with the Application" and silently do
  nothing.

---

## 6. Known limits

- Dropping a parameter replaces an item's coverage; no merge, no confirmation.
- Dragging works parameter to V&V item only. The reverse would need a change in the `EngineeringModel` plugin.
- The analysis check compares numbers only; it ignores non-numeric values and units.
- The Send button of the **stock** floating annotation window is gated on a permission evaluated once when the
  window opens, and is often dead. Use **Reply...** in the context menu instead, which reports a real error.
- Procedure steps are read-only in the tree; they are edited in the item dialog's Procedure tab.
- The views are column sets on one tree, not separate dockable panels, so two of them cannot be shown side by side.
- No baseline or snapshot of the VCD per issue yet, no bulk edit, and no Excel import.
