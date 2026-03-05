# LocalizedTMPText – where to add (Parent → TMP, Key, Style)

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
