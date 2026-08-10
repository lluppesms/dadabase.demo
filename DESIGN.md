---
name: Dad-A-Base
description: A playful dad-joke destination and engineering demonstration with a luminous, adaptive interface.
colors:
  fresh-green: "#2bcf6c"
  fresh-green-deep: "#0d9f4f"
  signal-green: "#35d07f"
  signal-green-strong: "#0b8f4d"
  sky-pulse: "#00b7c9"
  lime-highlight: "#d7f7a1"
  paper-mist: "#edf6f4"
  sky-wash: "#d9eff7"
  night-ink: "#08111f"
  midnight-deep: "#060d19"
  midnight-blue: "#11213a"
  card-light: "rgba(255, 255, 255, 0.84)"
  card-dark: "rgba(10, 25, 48, 0.84)"
  text-light: "#102a43"
  text-dark: "#eaf2ff"
typography:
  display:
    fontFamily: "Bebas Neue, Impact, Arial Black, sans-serif"
    fontSize: "clamp(2.4rem, 4.8vw, 4rem)"
    fontWeight: 400
    lineHeight: 1.02
    letterSpacing: "0.06em"
  body:
    fontFamily: "Space Grotesk, Segoe UI, Tahoma, Geneva, Verdana, sans-serif"
    fontWeight: 400
  label:
    fontFamily: "IBM Plex Mono, Courier New, Courier, monospace"
    fontWeight: 500
    letterSpacing: "0.06em"
rounded:
  tight: "12px"
  surface: "18px"
  pill: "999px"
spacing:
  compact: "0.5rem"
  control: "0.78rem 1.75rem"
  card: "2rem"
  page: "3rem 2rem"
components:
  button-primary:
    backgroundColor: "linear-gradient(130deg, #d7f7a1, #67d98f 45%, #2bcf6c)"
    textColor: "#09131f"
    rounded: "{rounded.pill}"
    padding: "{spacing.control}"
  button-primary-hover:
    backgroundColor: "linear-gradient(130deg, #e7ffb7, #8ce3ae 42%, #1ca95a)"
    textColor: "#09131f"
    rounded: "{rounded.pill}"
  joke-card:
    backgroundColor: "{colors.card-light}"
    textColor: "{colors.text-light}"
    rounded: "{rounded.surface}"
    padding: "{spacing.card}"
---

# Design System: Dad-A-Base

## Overview

**Creative North Star: "The Playful Signal Lab"**

Dad-A-Base pairs a practical joke-finding interface with the visual confidence of a working technical demonstration. The default system is bright and celebratory: Fresh Green and Sky Pulse activate a misty, cyan-washed field while translucent cards keep content legible and grounded. The mood is energetic without becoming a generic enterprise dashboard.

Display typography is emphatic and friendly; body and utility typography keep searching, filtering, and admin work readable. The system deliberately supports light and dark modes with the same signal palette, while the optional 90s theme is a fully separate celebratory escape hatch rather than a baseline for everyday product surfaces.

**Key Characteristics:**
- Luminous green-and-cyan signal color over light and deep-blue gradient fields.
- Ambiently lifted, translucent panels with soft blur and responsive motion.
- Bold uppercase display headlines balanced by pragmatic grotesk and mono utility text.
- Playful discovery cues that do not obscure real controls or product status.

## Colors

The palette reads as a modern signal system: green carries action and freshness, cyan supplies focus and atmospheric lift, and inky blue makes the dark theme feel deliberate rather than merely inverted.

### Primary
- **Fresh Green** (`#2bcf6c`): The main action gradient and category signal; it appears in active navigation, card accents, and primary calls to action.
- **Fresh Green Deep** (`#0d9f4f`): The darker terminal of green gradients and a stable link/accent color.
- **Signal Green** (`#35d07f`): A brighter supporting green used in luminous background effects.
- **Lime Highlight** (`#d7f7a1`): The high note in primary buttons and dark-mode focus states.

### Secondary
- **Sky Pulse** (`#00b7c9`): The cool signal color for focus rings, translucent state surfaces, and the page-field atmosphere.

### Neutral
- **Paper Mist** (`#edf6f4`) and **Sky Wash** (`#d9eff7`): The light-mode foundation beneath translucent white cards.
- **Night Ink** (`#08111f`), **Midnight Deep** (`#060d19`), and **Midnight Blue** (`#11213a`): The dark-mode gradient field and high-contrast structural backdrop.
- **Light Reading Ink** (`#102a43`) and **Dark Reading Light** (`#eaf2ff`): Primary reading colors for their respective themes.

### Named Rules
**The Signal, Not Spreadsheet Rule.** Use green and cyan to orient people toward actions, focus, and delight; do not turn the default experience into a muted, dense enterprise dashboard.

## Typography

**Display Font:** Bebas Neue, with Impact and Arial Black fallbacks.
**Body Font:** Space Grotesk, with Segoe UI and system fallbacks.
**Label/Mono Font:** IBM Plex Mono, with Courier fallbacks.

**Character:** The type system is direct and cheerful. Bebas Neue gives public headings an unmistakable, poster-like dad-joke energy; Space Grotesk makes prose and controls practical; IBM Plex Mono identifies navigation, categories, and technical utility language.

### Hierarchy
- **Display** (400, `clamp(2.4rem, 4.8vw, 4rem)`, 1.02): Uppercase page titles and the primary public moment.
- **Headline** (700, context-dependent): Section headings and major content regions, using the body family for denser workflows.
- **Body** (400, `1.1rem` on joke content, 1.8): Readable joke copy and supporting content.
- **Label** (500-600, `0.75rem` to `0.9rem`, `0.04em` to `0.06em`): Mono categories, navigation labels, and compact utility controls.

### Named Rules
**The Three-Voice Rule.** Use Bebas Neue for the big public statement, Space Grotesk for reading and operating, and IBM Plex Mono only for compact signals and labels.

## Layout

The default page container caps content at 1200px and uses `3rem 2rem` desktop padding, tightening to `2rem 1rem` at 768px and below. The fixed top bar spans the viewport; controls inside it are vertical icon-and-label tabs that become more compact on smaller screens. Content favors a single clear reading column, with flex-wrapped action groups that become full-width stacked controls on mobile.

Major surfaces use a comfortable rhythm: 1rem for clustered controls, 1.5rem for card separation, 2rem for controls and panels, and 3rem for page or history sections. Responsive rules preserve content width, prevent image overflow, and make primary action buttons full width at the mobile breakpoint.

## Elevation & Depth

The system uses **ambient lift**. Light-mode cards float over a misty gradient field with translucent backgrounds, a 1px low-contrast border, backdrop blur, and soft shadows. Dark mode keeps the same hierarchy with inky surfaces and higher-contrast borders rather than abandoning depth. Hover elevation is brief and deliberate: cards rise by 2-4px, and primary buttons rise by 2px with a restrained scale increase.

### Shadow Vocabulary
- **Soft Lift** (`0 8px 18px rgba(13, 33, 57, 0.08)`): Small contextual surfaces and category chips.
- **Panel Lift** (`0 14px 30px rgba(13, 33, 57, 0.14)`): Default primary panels and buttons.
- **Hero Lift** (`0 24px 48px rgba(8, 16, 29, 0.24)`): Hovered cards and moments that need clear emphasis.

### Named Rules
**The Float With Purpose Rule.** Use elevation to confirm interaction or isolate a meaningful surface; do not layer cards inside cards just to create visual activity.

## Shapes

Default surfaces use relaxed rounded rectangles: 18px for primary cards, 12px for compact panels and fields, and fully pill-shaped primary buttons and category labels. Borders remain thin and translucent. The top navigation is the exception, using 14px rounded top corners and a small active-state pointer to connect the selected tab to the page below.

The 90s theme intentionally breaks this form language with hard black borders, ridge effects, pixelated imagery, and high-contrast primary colors. It is an alternate mode, not a source for default-shape decisions.

## Components

### Buttons
- **Character:** Bright, celebratory calls to action that remain plainly actionable.
- **Shape:** Pill (`999px`) for the primary `.btn-primary` action.
- **Primary:** Lime-to-green gradient, `#09131f` text, `0.78rem 1.75rem` padding, and a thin dark-green border.
- **Hover / Focus:** Lift by 2px, brighten the gradient, and use the established cyan focus ring. Respect reduced-motion preferences.

### Cards / Containers
- **Character:** Glassy, gently elevated frames for the joke rather than decorative page furniture.
- **Corner Style:** 18px for `.JokeCard`; 16px for image panels.
- **Background:** Translucent white in light mode; translucent navy in dark mode, with backdrop blur.
- **Shadow Strategy:** Panel Lift at rest, Hero Lift on hover.
- **Border:** 1px from the active theme's surface border token.
- **Internal Padding:** 2rem for primary joke cards.

### Inputs / Fields
- **Style:** 12px corners, a translucent white or deep-blue fill, and a 1px surface border.
- **Focus:** Signal Green border with a soft cyan outer focus ring; dark mode uses Lime Highlight where MudBlazor outlines are focused.
- **Validation:** Green for valid fields, `#f56565` for invalid fields, each reinforced by a soft 4px halo.

### Navigation
- **Character:** A fixed, deep-blue gradient command strip with category-like utility labels.
- **Default:** Bebas Neue product title; IBM Plex Mono uppercase nav labels with white iconography.
- **Hover / Active:** Hovered tabs lift on a translucent white field. The active tab uses Fresh Green, a soft shadow, and a downward pointer.
- **Mobile:** Horizontal padding and label size reduce at 768px; the title drops to 1.5rem and its forced width is removed.

### Joke Card
- **Character:** The signature content vessel: a focused, glassy stage for a single joke.
- **Signature details:** A 4px green-lime-cyan top rail, a mono pill category label, and a gentle hover lift.

## Do's and Don'ts

### Do:
- **Do** use Fresh Green for primary action and active state while reserving Sky Pulse for focus, subtle interactive fields, and ambient background energy.
- **Do** preserve the default light/dark system as paired experiences with the same hierarchy and readable contrast.
- **Do** use relaxed translucent panels sparingly over the gradient field, with established 12px or 18px radii.
- **Do** maintain the 768px mobile behavior that stacks major primary actions and protects image width.
- **Do** keep the 90s theme visually self-contained as a deliberate celebratory alternate mode.

### Don't:
- **Don't** replace the green-and-cyan signal language with muted dashboard neutrals or data-grid-heavy composition on public discovery surfaces.
- **Don't** use Bebas Neue for dense paragraph text or IBM Plex Mono for long-form reading.
- **Don't** add nested floating cards or shadows that have no interaction or hierarchy purpose.
- **Don't** introduce motion that ignores the global `prefers-reduced-motion` safeguard.
- **Don't** blend the 90s theme's hard-border, blinking, or pixel-art language into the default system.