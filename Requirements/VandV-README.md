# Verification and Validation (part of the Requirements plugin)

A capability of the Requirements plugin that turns an engineering model into a working **Verification Control Document (VCD)**:
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
| What performs it | a `BinaryRelationship` categorized **`performedBy`**, from the item to a shared **activity** |
| Shared activity | `Requirement` categorized `VnVActivity`; its short-name is the activity number. It lives inside its report, or in the `VNV` specification when it belongs to none |
| Report (deliverable) | `RequirementsSpecification` categorized `VnVReport`; the activities it records are contained in it, so the report is one standalone, exportable thing whose groups can serve as chapters |
| Non-conformances | `ReviewItemDiscrepancy`, `RequestForDeviation`, `RequestForWaiver` on the `EngineeringModel` |

Because a V&V item is itself a requirement, one requirement can be covered by many items (one per method, level or
stage gate combination), which is exactly what ECSS-E-ST-10-02 expects of a verification strategy.

### 1.1a Shared activities and reports

In practice one task verifies many requirements: producing the mass budget closes out every mass requirement,
one power-speed curve covers every speed and power requirement, and a FAT report carries dozens of activities.
A **shared activity** models that task once:

- The activity carries, **once**, everything that describes the task: description, method, stage gate, level,
  facility, external party, dates, plan/procedure references, **the procedure steps**, and the **execution record**
  (status, actual date, result, evidence).
- Each V&V item points at the activity that performs it (`performedBy`). The item keeps only what is genuinely
  per requirement: the `verifies`/`validates` link, the acceptance criteria, the coverage links, the analysis
  check, compliance and close-out.
- **Derivation**: any planning/execution attribute the item leaves empty is read from its activity, everywhere -
  the register, the roll-up, the stage filter, the VCRM, the built-in rules and the export. The item's own value
  always wins, so an item can override its activity. Compliance, close-out and acceptance criteria never derive.
- A **report** is its own `RequirementsSpecification` ("FAT Report - Electrical"), and the activities it records
  live inside it - one standalone document you can browse, group into chapters, and export as a whole. A stage gate
  can have any number of reports; the report's short-name is its document reference, offered in the plan/evidence
  pickers so a document is referenced one way. **Report vs plan reference:** the report is the *output* document the
  results are recorded in; the plan reference points into the *input* document (the verification plan) that says
  what will be verified how.

This mirrors ECSS-E-ST-10-02's own separation between the per-requirement VCD entry and the *verification task*
that executes it: one task closing out many VCD lines is exactly the standard's model. It is still all stock
ECSS-E-TM-10-25 concepts, so nothing changes for other COMET clients.

### 1.2 Four statuses that are deliberately not the same thing

This trips people up, so the plugin keeps them apart everywhere, including in the export:

| Question | Field | Example |
|---|---|---|
| Did the activity happen, and what did it show? | **Execution status** `vnv_status` | `Executed`, `Passed`, `Failed` |
| Does the design meet the requirement? | **Compliance** `vnv_compliance` | `Compliant`, `Partially Compliant` |
| Has the customer accepted this item, and why? | **Close-out** `vnv_closed` + reason | `Closed`, "accepted at CDR under RFW-004" |
| Is the *requirement* finished, or is more owed later? | **Requirement closure** `vnv_closure` | `Closes Out Requirement`, `Further V&V Required` |

A test can pass while the requirement is only partly met. That is precisely the case a waiver exists for, and a tool
that collapses these into one column cannot represent it.

The fourth row is the one people miss. ECSS-E-ST-10-02 lets a requirement be verified by a combination of methods,
levels and stages, so closing an *item* is not the same as closing the *requirement*. A mass requirement analysed
against the budget at PDR and weighed at FAT has two items, and only the second closes the requirement out. Until
some item says so, the plugin treats the requirement as **undefined** and says so loudly in the stage gate review
(section 3.7). That is deliberate: a plan with no end is a planning gap, not a finished plan.

Compliance and closure are both recorded per item, and items are per stage gate, so both can differ from one gate to
the next. A design can be compliant at PDR and fall short at FAT, and the review shows that per gate rather than
averaging it away.

---

## 2. Getting started

### 2.1 Set up the reference data (once per model)

The plugin needs parameter types, categories and rules in the model's RDL chain. Nothing is assumed to be there.

1. **Requirements ribbon tab > Verification & Validation > Set up V&V**, and pick the model.
2. It reports exactly what is missing and what it will create.
3. Confirm, then **define your stage gates**: a dialog shows them as a table, one row per gate, prefilled with a
   common example list you are expected to replace. Add, Remove, Up and Down manage the rows; the order is the
   project's review order. They become the gates every activity is planned against, the VCRM columns and the stage
   filter. "Stage gate" is deliberately the generic term (INCOSE calls the same thing a decision gate); it is not a
   milestone in the acquisition sense, nor the ECSS verification stage, which is a space-specific axis. Cancel here
   aborts the whole set-up; nothing is written.
4. It writes only what is absent, so it is safe to run again after an upgrade. Gates can be **added** later by
   editing the `V&V Stage Gate` parameter type in the reference data. Renaming a gate does not move the items already
   planned against it: they keep the old text and the matrix simply shows an extra column for it. See §4.3.

The check walks the whole chained RDL (`QueryParameterTypesFromChainOfRdls` and friends), so anything already
defined in a site RDL is reused rather than duplicated. If you lack write access to the RDL, the dialog says so and
lists what an administrator must create.

> **After upgrading the plugin, run Set up V&V again.** New releases add parameter types and categories. Items
> created before an upgrade keep working; the new columns simply read empty until they are filled in. Note that the
> shared-activity release added the `VnVActivity` and `performedBy` categories to the set every item write checks,
> so on a model seeded by an older release, **Create/Edit V&V Item is refused until Set up V&V has run again**.

### 2.2 Open the register

**Requirements ribbon tab > Verification & Validation > Open VCD**, and pick the iteration. Set up V&V and Open VCD
sit together in the one **Verification & Validation** group. The register docks in the middle document area, next to
your model browsers.
The V&V capability is part of the Requirements plugin, so it is available whenever requirements are.

The tree mirrors the stock Requirements browser (specification > group > requirement) and nests the V&V items under
the requirement each one covers, so the two browsers read alike.

> The stock **Requirements browser hides the V&V specifications** (the `VNV` plan and the report specifications) by
> default, because they hold verification bookkeeping rather than requirements to be engineered. The
> **Show V&V Specifications** tick box at the bottom, next to the simple-parameter-value and parametric-constraint
> ones, brings them back when you need to see them as plain requirements.

---

## 3. Daily use

### 3.1 Create a V&V item

Right-click a requirement > **Create V&V Item for this Requirement**. One dialog, six tabs:

| Tab | Holds |
|---|---|
| **Basic** | link type, short name, name, owner, **performed by** (the shared activity), method, stage gate, acceptance criteria, description |
| **Planning** | method, stage gate, acceptance criteria (kept in sync with Basic), level, criticality, plan reference, planned date, external party, coverage note |
| **Procedure** | procedure reference, preconditions, conditions, facility, and the ordered steps |
| **Execution** | status, actual date, result, evidence reference |
| **Compliance** | compliance status, the read-only analysis check, and the close-out record |
| **Coverage** | which parameter, element, options and states this activity actually measures |

On OK the plugin creates, in **one transaction**: the item, all its attributes, the `verifies` (or `validates`)
relationship, the `VNV` specification if it did not exist, and any coverage links, followed by the `performedBy`
link when an activity was picked.

**Link type** decides the verb. `verifies` checks the item against the requirement baseline; `validates` checks it
against the mission or stakeholder need. Changing it rewrites the suggested name, unless you have typed your own.

**Acceptance criteria from a constraint.** If the requirement carries `ParametricConstraint`s, a *from constraint*
picker appears under the criteria box. Pick a whole constraint or a single relational expression, press **Use**, and
its expression text is appended to the criteria. If that expression is bound to a parameter, the coverage link is
set at the same time, which is what later enables the analysis check.

**Performed By.** The Basic tab's *Performed By* picker points the item at the shared activity that performs it -
pick it first. Method and stage gate then stop being mandatory **when the activity actually states them** (they stay
overridable per item); the Procedure and Execution tabs announce that their content is inherited and are only for
per-requirement additions; and the *Plan Reference* and *Evidence Ref.* boxes offer the values already used in the
model (and the defined reports), so a reference is picked once instead of retyped and mistyped. The old free-text
*Activity No.* box is gone: the activity number is the activity's short name, and the column shows it.

**Plan Reference vs Evidence Ref.** The *plan reference* points into the verification **plan**, the input document
that says what will be verified how; write it once on the activity and the items inherit it. Leave it empty if the
project keeps no such document, nothing flags it. The *evidence reference* is the record backing a result, in
practice the **report** the activity is recorded in: you normally never type it per item, because *Apply Activity
Result* prefills it from the activity (falling back to its report reference) and writes it onto every item it
updates.

### 3.1a Work with shared activities

Planning the work happens in its own panel: **Requirements ribbon tab > Verification & Validation > Open
Activities**. It sits next to Open VCD deliberately but is a different job: the Activities panel is about the work
and the documents it is recorded in, the register (VCD) is about judging requirements. Its tree is
report > activity > **the activity's procedure steps**, because this panel shows the work as it will be carried out.
How many V&V items an activity covers is its *V&V Items* column, the items themselves are listed on the activity
dialog's V&V Items tab, and **Highlight Items** points them out in the register.

The panel carries the stock browser toolbar and header banner, the same ones the Requirements browser has: a
**Create** split button (its list is exactly the context menu's create entries), Edit, Inspect, Delete, and the
filter and search buttons, above the model / iteration / data-source / person / domain line. Everything specific to
V&V, linking, applying a result, highlighting and exporting a report, lives in the **context menu**.

**Create a report first**: **Create a Report**. A report is a specification of its own, so it is created
deliberately, with a document reference (its short name), a title and an owner. Nothing is auto-created from a typed
name any more.

**Create an activity**: **Create an Activity** (select a report first and it is preselected). The dialog has
four tabs: **Basic** (activity number = short name, name, owner, **report**, method, stage gate, level, description,
facility, external party, dates, plan reference), **Procedure** (the steps, written once here instead of per
requirement), **Execution** (status, actual date, result, evidence), and **V&V Items** (read-only: which items this
activity performs and which requirement each verifies). Method and stage gate are mandatory: every item the activity
performs inherits them. The *Report* picker offers only reports that exist; changing it on a later edit moves the
activity (the same container change the stock browser performs when a requirement is dragged onto another
specification), and clearing it moves the activity back to the V&V specification.

> **One activity is one execution.** An activity has one method and one stage gate because it is performed once and
> produces one execution record and one piece of evidence. Recurring work, a mass budget at PDR and again at CDR, is
> **one activity per gate** (`ACT_10`, `ACT_20`), each in its own report. That is also what lets the two runs carry
> different results. A requirement verified by several methods is likewise several V&V items, one per activity,
> which is exactly what ECSS-E-ST-10-02 expects of a verification strategy.

**Cover requirements that have no item yet**: select an activity, then **Create V&V Items for new Requirements**. Tick the
requirements (uncovered ones are unmarked, covered ones say so), choose the link type and owner, set the acceptance
criteria, press Create. One thin item per requirement is written in **one transaction**, each linked `verifies` to its
requirement and `performedBy` to the activity. This is the "thirty mass requirements, one mass budget" gesture.

Acceptance criteria stay per requirement, because they state the threshold one requirement is judged against and a
shared value can only ever be a default:

- The **Default Acceptance Criteria** box applies to every ticked requirement that states none of its own.
- Each row has its own box for the requirements judged against something else. **Fill Blanks With Default** copies the
  default into the empty ones so they can be edited from a filled-in starting point.
- Where a requirement carries a **`ParametricConstraint`**, the row shows a **from constraint** picker with a **Use**
  button, exactly as the V&V item dialog does. Nothing is filled in on your behalf: you pick the constraint (or one
  expression inside it) and press Use to copy it in.
- **Create stays disabled until every ticked requirement has criteria**, and the line next to it says how many are still
  missing. Items used to be creatable with none, save cleanly, and only then read as incomplete when opened.

**The old way still works.** Creating a V&V item per requirement in the register, with its own method, stage,
procedure and execution, is unchanged; those fields simply stop being mandatory once an activity supplies them. An
existing item is linked to an activity at any time, one at a time through the item dialog's *Performed By* picker or
in bulk through **Link Existing Items** (§3.1a). Items never point at a report directly: the chain is
item > activity > report, because the report is where the activity is recorded.

**Execute once, move everything**: record the run on the activity (as-run steps, status, evidence). When an edit
concludes the activity and open items hang off it, the browser offers **Apply Activity Result to its Items...**
(also on the context menu): a confirmation dialog lists every performed item, with the execution record prefilled
from the activity, and an optional **close-out** section (compliance, mandatory reason, who and when). Items with a
**violated analysis check**, an **open review request**, or that are **already closed** start out unticked and say
why - the bulk gesture cannot silently close what needs a human look. Execution may follow the activity; compliance
and close-out remain an explicit, per-requirement act, which is the plugin's standing discipline.

**Link items that already exist**: select an activity > **Link Existing Items**. Every V&V item in the model is
listed with what it verifies, its method and stage, **whether it carries a procedure of its own**, and which
activity performs it today. Tick and link: the items keep their acceptance criteria and coverage, and start
inheriting this activity's plan and execution record. An item performed by another activity is moved, never
duplicated.

**Highlight what an activity covers**: select an activity > **Highlight its V&V Items in the Register**. The items it
performs turn yellow in the V&V Register, which brings itself to the front and opens every group above them. The
register has to be open in the same iteration; nothing is re-opened behind your back.

**Export one report**: select a report > **Export Report**. One workbook for that single deliverable: its
activities, their procedures step by step, and the requirements they cover.

### 3.1b Requirement authors and V&V planners meet in the middle

The two roles rarely work at the same time. Whoever writes a requirement is expected to cover it, but does not yet
know which real-world task will do the verifying, so they create a V&V item and often type a procedure on it. The
V&V planner comes later and knows the campaign. The bridge is one command:

**V&V Register > right-click a V&V item > Create a V&V Activity from this Item...** The activity dialog opens
prefilled with that item's method, stage gate, level, description, facility, conditions and **its whole step list**.
Pick the report, press Create, and the plugin writes the activity, moves the procedure onto it, links the item to
it, and clears the item's own copy of the steps so one procedure never exists in two places.

From then on the ordinary flow applies: use **Link Existing Items** to point the other twenty mass requirements'
items at the same activity. The *Own Steps* column in that dialog is the overview of which items still carry a
hand-written procedure that ought to be folded in.

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

Activities and reports are not a view of the register: they have their own panel, **Open Activities** (§3.1a).

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

On the Compliance tab: set **Compliance**, set **Requirement Closure**, tick **Closed**, and state the **Reason**
(mandatory, the dialog refuses to save a closed item without one), plus who closed it and when.

Closing an item is not the same as finishing the requirement, so the dialog asks both questions:

- **Closed** means the work recorded on *this item* is finished and accepted.
- **Requirement Closure** means either `Closes Out Requirement` (nothing further is owed anywhere) or
  `Further V&V Required` (this item contributes, and another item is owed at a later stage gate).

The completeness rule then enforces the ECSS discipline:

- closed with no reason
- closed as `Partially Compliant` or `Non-Compliant` with **no closed waiver or deviation** conceding the shortfall
- closed while a review request against it is still open
- closed without saying whether it closes the requirement out or further V&V is required

### 3.7 Stage gate review, the coverage matrix (VCRM)

**Coverage Matrix** on the toolbar opens a live requirements-by-stage-gate matrix. Each cell says what that gate has
to decide about that requirement, and the last column gives the single verdict that answers "is this requirement
verified and validated".

| Cell state | Means | Derived from |
|---|---|---|
| `Closed out` | verified or validated here, nothing further owed | items at this gate all concluded, one of them says `Closes Out Requirement` |
| `Verified, more to come` | verified or validated here, but a later gate still owes work | items at this gate all concluded, none closes the requirement out |
| `Planned` | V&V sits at this gate but has not concluded | at least one item at this gate still open |
| `FAILED` | V&V here failed, or fell short with no accepted concession | the roll-up |
| `Not at this gate` | cannot be verified here, the work is planned later | no items here, items at a later gate |
| *(blank)* | already complete, nothing left to do | no items here, the requirement closed out earlier |
| `UNDEFINED` | **nobody has decided.** Either no V&V exists at all, or V&V exists but nothing ever closes the requirement out | the planning gap this view exists to expose |

Only `vnv_closure` is entered by hand. Everything else is derived from the items already in the register, so there is
no second plan to keep in step with the first.

Two controls turn the matrix into a gate review:

- **Stage gate** picks one gate. The grid narrows to that gate's state, what was done there, and the compliance
  recorded there, in separate sortable columns. That is the view to project in the review meeting.
- **Only requirements needing a decision** hides everything already decided, leaving the `UNDEFINED` rows. Drive that
  list to empty while planning, and at the end of the project the verdict column is the evidence that every
  requirement is verified and validated.

The header line counts each state at the selected gate and repeats the overall undefined count, so the number never
gets lost behind a filter.

Cells are colour coded to the three answers a review has to act on: **amber** for `UNDEFINED`, **red** for `FAILED`
and for a compliance shortfall, **green** for `Closed out`. Planned, deferred and verified-but-not-finished are left
plain on purpose, so there is something for the eye to land on.

Stage gates come from the RDL, not from a hard-coded list, plus any stage actually in use in the model. Define your
own stage gates in the RDL and they become columns with no rebuild. **Their order in the RDL is the project order**:
"earlier" and "later" gates, and therefore `Not at this gate` versus `UNDEFINED`, are read off that order.

### 3.8 Export

**Export VCD / VCRM** writes one workbook with six sheets:

| Sheet | Contents |
|---|---|
| **VCD** | one row per V&V item, 28 columns, every field ECSS-E-ST-10-02 Annex B lists, including requirement text, parent requirement, compliance, requirement closure, close-out and the analysis check. Planning and execution fields are read through the shared activity, so the per-requirement VCD stays complete even when the how is written once. Uncovered requirements appear in red. |
| **VCRM** | the stage gate review: requirements down, gates across, each cell carrying the state at that gate and the last column the requirement-level verdict. Colour coded, so undefined and failed rows are visible from across a meeting room. |
| **Activities** | one row per shared activity: number, name, report, planning, execution record, and the items and requirements it performs - the task-sheet view kept alongside the per-requirement VCD |
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

**Close-out**: `vnv_compliance`, `vnv_closure`, `vnv_closed`, `vnv_closeout_reason`, `vnv_closed_by`, `vnv_closed_on`

Every enumeration is read from the RDL at runtime, so a project can add its own methods, stage gates or levels
without touching code.

### 4.2 Categories

`VnVItem` (plus `VerificationItem` and `ValidationItem` as sub-categories), `VnVStep`, `VnVActivity`, `VnVReport`
(applied to the report specifications), `NCR`, and the relationship categories `verifies`, `validates`,
`coversOption`, `coversState`, `coversParameter`, `verifiedOn`, `hasStep`, `performedBy`.

The requirement category is auto-detected: the plugin matches `REQUIREMENT` or `REQ` case-insensitively, including
sub-categories that carry one of those as a super-category.

### 4.3 What you may and may not edit in the RDL

The plugin finds its reference data **by short name**, so the short names above are effectively reserved.

| Edit | Effect |
| --- | --- |
| Rename the **Name** of a parameter type or category | Safe. Only the label in the Reference Data browser changes. |
| Add an enumeration **value** (a new method, stage gate or level) | Safe and supported. Every enumeration is read from the RDL at runtime. |
| Rename an enumeration **value's Name** | Avoid. Values already stored on items keep the old text, and the roll-up and the built-in rules classify on the names declared in `VandVStatus`, `VandVCompliance` and `VandVStepResult`. |
| Rename a **ShortName**, or delete a parameter type or category | Breaks that attribute. See below. |

Renaming a short name looks exactly like a deletion to the plugin:

- **Set up V&V** reports the short name as missing and offers to create it again. Accepting that leaves you with two
  parameter types, the renamed one and a fresh one.
- **Create V&V Item** and **Edit V&V Item** are refused, with "The V&V reference data is not complete in this model
  yet", because writing an attribute whose parameter type cannot be resolved is a silent no-op that would drop what
  the user typed.
- Items that already carry values keep them, but those values point at the renamed type, so the plugin can no longer
  read them: the column is blank in the register and the export, the completeness rule reports "it has no stage gate",
  and the roll-up counts the item as open.

Nothing is lost, and renaming the short name back restores everything. A real delete is normally refused by the server
once any item references the type; it only succeeds while the type is unused.
---

## 5. Architecture

```
Rdl/           manifest, check-and-seed service, check result
Services/      coverage query, roll-up, close-out vocabulary, analysis checker,
               item creator, activity query/writer, coverage writer, annotation kinds/query/creator,
               workbook exporter
Rules/         the two IBuiltInRules
ViewModels/    browser, matrix, item dialog, activity dialog, bulk-items dialog, apply-activity dialog,
               annotation dialog, ribbon, rows
Views/         XAML, plus the tree node image selector in Selectors/
```

Points worth knowing before changing anything:

- **`VandVCoverageQuery` is the single source of truth** for what counts as coverage and how status rolls up. The
  browser, the matrix, the rules and the exporter all go through it, so they cannot disagree.
- **`VandVActivityQuery.EffectiveAttribute` is the single derivation point** for reading an item's attribute through
  the activity that performs it. Everything that reads planning or execution off an item goes through it; anything
  reading acceptance criteria, compliance or close-out deliberately does not.
- **Resolve the maps once, then read them.** Every relationship lookup here is a scan of `iteration.Relationship`, so
  anything iterating many items resolves the activity, the performed items, the procedure steps and the covering
  requirement up front (`VandVActivityQuery.QueryActivityMap` / `QueryPerformedItemsMap`,
  `VandVProcedureWriter.QueryStepsMap`, `VandVItemCreator.QueryCoveringMap`, `VandVCoverageModel.ActivityByItem`) and
  passes them down. The two-argument `EffectiveAttribute` overload exists for exactly this; the one-argument one is
  for single-item callers. Reading them per attribute per row is what made a large register stutter.
- **Both panels coalesce their refreshes.** One write echoes back an event per changed value, so the register buffers
  `SimpleParameterValue` updates and the Activities panel buffers its whole rebuild stream over a 200 ms window;
  every listener is filtered to this iteration, and removals are matched against the rows actually shown.
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
- Bulk edit exists only through a shared activity (create items for it, apply its result); there is no free
  multi-select bulk edit across arbitrary items.
- A report is created by typing its reference in the activity dialog, and an activity moves between reports by
  changing that field; renaming a report, deleting it and organizing chapters (groups) inside it are done in the
  stock Requirements browser (it is an ordinary `RequirementsSpecification`).
- No per-report export yet: the Activities sheet lists every activity with its report; a one-workbook-per-report
  export would be a natural next step.
- Reading an item's derived attributes scans the iteration's relationships per item; on registers of many hundreds
  of items the Activities view and export may warrant a cached item-to-activity map.
- No baseline or snapshot of the VCD per issue yet, and no Excel import.
