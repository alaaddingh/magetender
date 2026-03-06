# Localization – Editor checklist

## Re-hook after pull (refs that get lost)

If ingredients don’t translate or tooltips don’t show after pulling from main, re-assign these in the Inspector.

### Ingredient tooltip – title and description (MixScene)

Find the GameObject that has **Ingredient Hover Snap UI** (e.g. on the mix/order canvas). To get the **ingredient title and description** to show in the tooltip, assign:

| Field | What to assign |
|-------|----------------|
| **Tooltip Root** | The GameObject that is the tooltip panel (parent of the name/desc/effect texts). |
| **Tooltip Name Text** | TMP_Text where the **ingredient name (title)** appears. |
| **Tooltip Desc Text** | TMP_Text where the **ingredient description** appears. |
| **Tooltip Effect Text** | TMP_Text for the X/Y effect line. |

Ingredient translation is automatic (loads `Data/Ingredients` or `Data/Ingredients_es` from LanguageManager); it only works if these refs are set.

### Mood marker tooltips (MixScene – Assess panel)

Find each GameObject that has **Mood Marker Tooltip** (e.g. on Satisfied / Neutral / Angry mood icons).

| Field | What to assign |
|-------|----------------|
| **Tooltip Root** | The GameObject that is the tooltip panel. |
| **Tooltip Text** | TMP_Text that shows the label (from UIStrings, e.g. `mood_tooltip_label`). |
| **Label Override** | (Optional) Leave empty to use `mood_tooltip_label`, or set a UIStrings key. |

---

## LocalizedTMPText – where to add (Parent → TMP, Key, Style)

Add the **Localized TMP Text** component to the GameObject in the **"Object"** column (the one that has the TextMeshPro text). Set **Key** and **Style** as in the table.

---

## TitleScreen

| Object (select this in Hierarchy) | Parent (to find it) | Key | Style |
|----------------------------------|----------------------|-----|--------|
| **Text (TMP)** under **Start** | Start | `start_button` | Button |
| **Text (TMP)** under **BackToMenu** | BackToMenu | `back_button` | Button |
| **Text (TMP)** under **Options** | Options | `options_button` | Button |
| **Text (TMP)** under **Credits** | Credits | `credits_button` | Button |

---

## MixScene

| Object (select this in Hierarchy) | Parent (to find it) | Key | Style |
|----------------------------------|----------------------|-----|--------|
| **Text (TMP)** under **BrewButton** | BrewButton | `begin_brew_button` | Button |
| **Text (TMP)** under **NextDayButton** | NextDayButton | `next_day_button` | Button |
| **Text (TMP)** under **NextButton** (Panel_DayCounter / day screen) | NextButton | `next_button` | Button |
| **Text (TMP)** under **NextButton** (Order screen) | BaseNextButton | `next_button` | Button |
| **Text (TMP)** under **PickGlassNextButton** | PickGlassNextButton | `next_button` | Button |
| **Text (TMP)** under **IngredientNextButton** | IngredientNextButton | `next_button` | Button |
| **Text (TMP)** under **BaseBackButton** | BaseBackButton | `back_button` | Button |
| **MoodProfileText** (mood graph title) | (under Graph / Assess panel) | `mood_profile_title` | MoodGraphTitleOrDescription |
| **Instructions** (“Hover icons for mood details”) | (under Graph / Assess panel) | `mood_graph_hint` | MoodGraphHint |
| **Text (TMP)** under **ContinueButton** (Assess screen) | ContinueButton | `continue_button` | Button |
| **Text (TMP)** under **NextButton** (day counter → Continue) | NextButton (Panel_DayCounter) | `continue_button` | Button |
| **Text (TMP)** under **FightButton** (Assess panel, when customer is angry) | FightButton | `fight_button` | Button |

If you have a separate description block for the mood graph (long “Adjust your customer’s mood…” text), add **Localized TMP Text** there with Key `mood_graph_description` and Style **MoodGraphTitleOrDescription**.

---

## QTECombatScene

| Object (select this in Hierarchy) | Parent (to find it) | Key | Style |
|----------------------------------|----------------------|-----|--------|
| **AttackLabel** | (attack bank area) | `attack_label` | None |
| **DefendLabel** | (defend bank area) | `defend_label` | None |
| **PlayerHealthLabel** | (player UI) | `player_label` | None |
| **CustomerHealthLabel** | (customer UI) | `customer_label` | None |
| **Text (TMP)** under **BackToBarButton** | BackToBarButton | `to_bar_button` | Button |

---

**Tip:** In Hierarchy, expand the parent (e.g. **Start**, **BrewButton**), then select the child that shows the label (often **Text (TMP)**). Add Component → **Localized TMP Text**, then set Key and Style.
