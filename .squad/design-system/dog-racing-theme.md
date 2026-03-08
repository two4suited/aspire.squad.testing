# 🏁 DogTeams Design System: Racing Champions Edition

## Executive Summary

**Theme Philosophy:** Merge the adrenaline and precision of motorsports with the loyalty, diversity, and athleticism of racing dogs. The visual language celebrates speed, competition, and the bond between handler and athlete.

**Target Emotion:** Energetic, professional, trustworthy, exciting

**Design Keywords:** VELOCITY • PRECISION • CHAMPIONSHIP • PACK MENTALITY

---

## 🎨 Color Palette

### Primary Colors

#### **Velocity Blue** 🏁
- **Hex:** `#0066FF` (racing blue)
- **RGB:** rgb(0, 102, 255)
- **Use:** Primary actions, headers, active states, racing accents
- **Psychology:** Speed, trust, professionalism
- **Inspiration:** Formula racing liveries, champion banners

**Variants:**
- `velocity-50`: `#E6F0FF` (backgrounds)
- `velocity-100`: `#CCE0FF` (hover states)
- `velocity-600`: `#0052CC` (button hover)
- `velocity-900`: `#003380` (dark text)

#### **Champion Gold** 🥇
- **Hex:** `#FFB300` (gold medal)
- **RGB:** rgb(255, 179, 0)
- **Use:** Highlights, achievements, premium features, winner badges
- **Psychology:** Excellence, achievement, warmth
- **Inspiration:** Winner's trophies, dog coat tones (golden retrievers)

**Variants:**
- `champion-50`: `#FFF8E6`
- `champion-400`: `#FFC933`
- `champion-700`: `#CC8F00`

### Secondary Colors

#### **Checkered Black** 🏴
- **Hex:** `#1A1A1A` (carbon fiber black)
- **RGB:** rgb(26, 26, 26)
- **Use:** Text, borders, racing flag accents
- **Pattern Use:** Checkered patterns for dividers, finish lines

**Variants:**
- `checkered-50`: `#F5F5F5` (light backgrounds)
- `checkered-700`: `#404040` (secondary text)
- `checkered-900`: `#0D0D0D` (pure black)

#### **Track Gray** 🛣️
- **Hex:** `#8A8A8A` (asphalt gray)
- **RGB:** rgb(138, 138, 138)
- **Use:** Disabled states, secondary text, borders
- **Inspiration:** Race track surfaces, neutral zones

#### **Paws Brown** 🐾
- **Hex:** `#8B6F47` (warm earth tone)
- **RGB:** rgb(139, 111, 71)
- **Use:** Dog-related elements, warm accents, organic components
- **Psychology:** Loyalty, natural, approachable
- **Inspiration:** Dog coat colors, leather collars

**Variants:**
- `paws-100`: `#F5F1EC`
- `paws-500`: `#A3845A`
- `paws-800`: `#6B5738`

### Semantic Colors

#### **Victory Green** ✅
- **Hex:** `#00C853` (success)
- **RGB:** rgb(0, 200, 83)
- **Use:** Success states, confirmations, winning status

#### **Caution Yellow** ⚠️
- **Hex:** `#FFD600` (pit lane warning)
- **RGB:** rgb(255, 214, 0)
- **Use:** Warnings, pending states, alerts

#### **DNF Red** 🚫
- **Hex:** `#FF1744` (critical/danger)
- **RGB:** rgb(255, 23, 68)
- **Use:** Errors, delete actions, disqualifications

---

## ✍️ Typography Hierarchy

### Font Families

#### **Primary: Inter** (Display & Body)
- **Source:** Google Fonts
- **Rationale:** Clean, modern, excellent readability, racing/tech aesthetic
- **Fallback:** `-apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Helvetica', sans-serif`

#### **Secondary: Bebas Neue** (Racing Headers)
- **Source:** Google Fonts
- **Rationale:** Bold, condensed, reminiscent of racing numbers and pit boards
- **Use:** Hero text, race results, big numbers (lap times, dog IDs)
- **Fallback:** `'Impact', 'Arial Black', sans-serif`

### Type Scale

| Element | Font | Size | Weight | Line Height | Use Case |
|---------|------|------|--------|-------------|----------|
| **Hero** | Bebas Neue | 72px | 700 | 1.1 | Landing page headlines |
| **H1** | Bebas Neue | 48px | 700 | 1.2 | Page titles |
| **H2** | Inter | 36px | 700 | 1.3 | Section headers |
| **H3** | Inter | 28px | 600 | 1.4 | Card titles |
| **H4** | Inter | 20px | 600 | 1.5 | Sub-sections |
| **Body Large** | Inter | 18px | 400 | 1.6 | Feature text |
| **Body** | Inter | 16px | 400 | 1.5 | Standard text |
| **Body Small** | Inter | 14px | 400 | 1.5 | Secondary text |
| **Caption** | Inter | 12px | 500 | 1.4 | Labels, metadata |
| **Racing Number** | Bebas Neue | 64px | 700 | 1 | Dog/Team IDs |

### Typography Usage Guidelines

- **Headers:** Use Bebas Neue for impact, Inter for readability
- **Body text:** Always Inter for legibility
- **Numbers/Stats:** Bebas Neue for racing aesthetic (lap times, positions, IDs)
- **Buttons:** Inter, 14px, weight 600, uppercase letter-spacing 0.5px
- **Navigation:** Inter, 16px, weight 500

---

## 🧩 Component Visual Language

### 1. Buttons

#### **Primary Button (Velocity Blue)**
```css
background: #0066FF
color: white
padding: 12px 24px
border-radius: 8px
font: Inter 14px, weight 600
letter-spacing: 0.5px
hover: background #0052CC, transform: translateY(-2px)
shadow: 0 4px 12px rgba(0, 102, 255, 0.3)
```

**Racing Accent:** Add 3px left border with checkered pattern on hover

#### **Secondary Button (Outline)**
```css
background: transparent
border: 2px solid #0066FF
color: #0066FF
padding: 10px 22px
border-radius: 8px
hover: background #E6F0FF
```

#### **Success Button (Victory Green)**
```css
background: #00C853
color: white
icon: checkered flag icon
```

#### **Danger Button (DNF Red)**
```css
background: #FF1744
color: white
icon: X or stop sign
```

#### **Racing Flag Button** (Special)
```css
background: linear-gradient(45deg, #1A1A1A 25%, white 25%, white 50%, #1A1A1A 50%, #1A1A1A 75%, white 75%, white)
background-size: 16px 16px
color: white (with text-shadow for readability)
border: 2px solid #FFB300 (gold accent)
use: Final submission, race start, championship actions
```

### 2. Cards

#### **Team Card**
```
Layout: Vertical
Background: White
Border: 1px solid #E0E0E0
Border-radius: 12px
Shadow: 0 2px 8px rgba(0, 0, 0, 0.08)
Hover: Shadow 0 8px 24px rgba(0, 102, 255, 0.15), border #0066FF

Header:
  - Racing stripe (4px height, velocity blue gradient)
  - Team name (H3, Inter 28px bold)
  - Team ID badge (Bebas Neue, gold background)

Body:
  - Dog count with paw icon
  - Owner info with user icon
  - Stats grid (races, wins) in Bebas Neue

Footer:
  - Action buttons (Edit, View Details)
  - Last updated timestamp (caption)
```

#### **Dog Profile Card**
```
Layout: Horizontal (image + info)
Background: White with subtle paws-brown-50 gradient
Border: 2px solid #8B6F47 (paws brown)
Border-radius: 16px
Shadow: 0 4px 16px rgba(139, 111, 71, 0.1)

Left: Dog photo (128x128, circular)
  - Gold border (3px) for champions
  - Breed badge overlay

Right: Info section
  - Name (H3, Inter bold)
  - Racing number (Bebas Neue 64px, velocity blue)
  - Stats: Age, Breed, Record
  - Status indicator (racing, retired, training)

Special: Champion dogs get gold medal icon top-right
```

#### **Owner Card**
```
Layout: Compact horizontal
Background: White
Border-left: 4px solid #0066FF
Padding: 16px
Border-radius: 8px

Content:
  - Avatar (48x48, circular)
  - Name + email
  - Dog count badge (gold pill)
  - Contact button (secondary style)
```

### 3. Navigation/Header

#### **Top Navigation Bar**
```
Background: #1A1A1A (checkered black)
Height: 72px
Border-bottom: 4px solid #0066FF (racing stripe)

Logo Area (left):
  - "DogTeams" in Bebas Neue 32px
  - Paw + racing flag icon combo
  - Color: White with gold accent

Navigation Links (center):
  - Inter 16px, weight 500
  - Color: White
  - Hover: Gold underline (3px, #FFB300)
  - Active: Velocity blue background pill

User Menu (right):
  - Profile avatar (40x40)
  - Notifications bell icon
  - Settings gear icon
```

#### **Racing Stripe Divider**
```
Use between sections
Height: 4px
Background: Linear gradient (velocity blue → champion gold)
Optional: Checkered pattern overlay at 20% opacity
```

### 4. Icons & Illustrations

#### **Icon Style**
- **Style:** Outlined (2px stroke), rounded corners
- **Size:** 24x24 (standard), 32x32 (featured), 16x16 (inline)
- **Color:** Inherit from context (velocity blue for primary, track gray for secondary)

#### **Custom Icon Set**
1. **Dog Silhouettes:** 5 breed variants (greyhound, husky, shepherd, retriever, terrier)
2. **Racing Icons:** Checkered flag, finish line, stopwatch, trophy, medal
3. **Action Icons:** Paw print, bone, collar, leash
4. **UI Icons:** Add, edit, delete, search, filter, sort

#### **Illustration Style**
- **Approach:** Flat 2D with subtle gradients
- **Color:** 2-3 colors max (velocity blue + champion gold + white)
- **Use:** Empty states, error pages, onboarding
- **Examples:**
  - Empty team list: Dog with checkered flag looking curious
  - 404 page: Dog crossed finish line but wrong track
  - Success: Dog with winner's wreath and trophy

---

## 📐 Spacing & Layout

### Spacing Scale (8px base unit)
```
xs: 4px    (tight spacing, icon gaps)
sm: 8px    (compact elements)
md: 16px   (standard spacing)
lg: 24px   (section spacing)
xl: 32px   (major sections)
2xl: 48px  (page sections)
3xl: 64px  (hero spacing)
```

### Grid System
- **Container max-width:** 1280px
- **Columns:** 12-column grid
- **Gutter:** 24px
- **Breakpoints:**
  - Mobile: 320px - 767px
  - Tablet: 768px - 1023px
  - Desktop: 1024px+

### Border Radius Scale
```
sm: 4px   (tags, badges)
md: 8px   (buttons, inputs)
lg: 12px  (cards)
xl: 16px  (modals, special cards)
full: 9999px (pills, avatars)
```

---

## 🏁 Page Mockups

### 1. Dashboard/Home Page

**Layout:** Grid of team cards

**Header Section:**
```
┌─────────────────────────────────────────────────────────┐
│ [Racing stripe - blue to gold gradient, 4px]            │
├─────────────────────────────────────────────────────────┤
│                                                          │
│   🏁 MY RACING TEAMS              [+ New Team] Button   │
│   Bebas Neue 48px                 Velocity Blue         │
│                                                          │
│   Stats Bar: 12 Teams | 47 Dogs | 3 Championships      │
│   Inter 16px, Track Gray                                │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

**Filter/Sort Bar:**
```
[Search teams...] [Filter: All ▼] [Sort: Recent ▼]
```

**Team Grid (3 columns):**
```
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│[Blue stripe]│ │[Blue stripe]│ │[Blue stripe]│
│             │ │             │ │             │
│ Storm       │ │ Lightning   │ │ Thunder     │
│ Runners #12 │ │ Pack #07    │ │ Hounds #03  │
│             │ │             │ │             │
│ 🐕 8 Dogs   │ │ 🐕 6 Dogs   │ │ 🐕 10 Dogs  │
│ 👤 J. Smith │ │ 👤 M. Jones │ │ 👤 K. Brown │
│             │ │             │ │             │
│ 🏆 5 Wins   │ │ 🏆 3 Wins   │ │ 🏆 12 Wins  │
│             │ │             │ │             │
│[Edit] [View]│ │[Edit] [View]│ │[Edit] [View]│
└─────────────┘ └─────────────┘ └─────────────┘
```

**Visual Enhancements:**
- Hover: Card lifts with blue shadow
- Champion teams: Gold medal icon top-right
- Recent activity badge: "New race 2h ago" in caution yellow

---

### 2. Dog Profile Page

**Hero Section:**
```
┌───────────────────────────────────────────────────────────┐
│ [Full-width racing stripe background, velocity blue]      │
│                                                            │
│   ┌────────┐                                              │
│   │  Dog   │  BOLT                    #42                │
│   │ Photo  │  Greyhound               Bebas Neue 64px    │
│   │ 200px  │  3 Years Old             Velocity Blue     │
│   └────────┘                                              │
│   [Gold border if champion]                                │
│              Status: 🏁 RACING        Record: 15-3-2      │
│              Owner: Jane Smith        Win Rate: 83%       │
│                                                            │
│   [Edit Profile] [View History] [Retire]                  │
│                                                            │
└───────────────────────────────────────────────────────────┘
```

**Stats Grid:**
```
┌──────────────────────────────────────────────┐
│  RACING STATISTICS                           │
│  Inter 20px, Weight 600                      │
├──────────────────────────────────────────────┤
│                                              │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│  │   15    │ │    3    │ │    2    │       │
│  │  WINS   │ │  PLACE  │ │  SHOW   │       │
│  │ Bebas   │ │  Bebas  │ │  Bebas  │       │
│  └─────────┘ └─────────┘ └─────────┘       │
│                                              │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│  │ 32.4s   │ │  83%    │ │   🥇   │       │
│  │BEST TIME│ │WIN RATE │ │CHAMPION│       │
│  └─────────┘ └─────────┘ └─────────┘       │
│                                              │
└──────────────────────────────────────────────┘
```

**Recent Races Timeline:**
```
┌─────────────────────────────────────────────┐
│ RECENT RACES                                │
├─────────────────────────────────────────────┤
│                                             │
│ ● 2024-01-15  🥇 1st Place  32.8s          │
│   City Championship - Track A              │
│                                             │
│ ● 2024-01-08  🥈 2nd Place  33.1s          │
│   Regional Qualifier - Track B             │
│                                             │
│ ● 2024-01-01  🥇 1st Place  32.4s (PR!)    │
│   New Year Sprint - Track A                │
│                                             │
│ [View Full History]                         │
└─────────────────────────────────────────────┘
```

**Visual Enhancements:**
- Personal record (PR) times in champion gold
- Win/loss indicators with colored dots
- Interactive timeline with hover states
- Print-friendly race certificate option

---

### 3. Team Management Page

**Split Layout (Owner + Dogs):**

```
┌────────────────────────────────────────────────────────────┐
│ TEAM: Storm Runners #12               [Edit] [Delete]      │
│ Bebas Neue 36px                                            │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ OWNER                                   DOGS (8)           │
│                                                            │
│ ┌──────────────────────┐               ┌────────────┐    │
│ │ 👤 Jane Smith        │               │ BOLT  #42  │    │
│ │ jane@example.com     │               │ Greyhound  │    │
│ │ 📞 555-1234          │               │ 15-3-2     │    │
│ │ 🐕 8 Dogs            │               │ [View] [Edit]   │
│ │                      │               └────────────┘    │
│ │ [Contact] [Edit]     │                                  │
│ └──────────────────────┘               ┌────────────┐    │
│                                         │ FLASH #38  │    │
│                                         │ Husky      │    │
│ [+ Add Owner]                           │ 8-5-1      │    │
│                                         │ [View] [Edit]   │
│                                         └────────────┘    │
│                                                            │
│                                         ... 6 more dogs    │
│                                                            │
│                                         [+ Add Dog]        │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Quick Actions Panel:**
```
┌──────────────────────────────┐
│ TEAM ACTIONS                 │
├──────────────────────────────┤
│ 🏁 Register for Race         │
│ 📊 View Team Stats           │
│ 📄 Generate Report           │
│ 🏆 Championship History      │
│ ⚙️ Team Settings             │
└──────────────────────────────┘
```

**Visual Enhancements:**
- Drag-and-drop to reorder dogs
- Quick-add modal with racing stripe header
- Bulk actions: "Select all" for batch operations
- Team color customization (accent stripe)

---

## 🎭 Patterns & Motifs

### Checkered Flag Pattern
**Usage:** Dividers, victory states, loading indicators
```css
background-image: 
  linear-gradient(45deg, #1A1A1A 25%, transparent 25%),
  linear-gradient(-45deg, #1A1A1A 25%, transparent 25%),
  linear-gradient(45deg, transparent 75%, #1A1A1A 75%),
  linear-gradient(-45deg, transparent 75%, #1A1A1A 75%);
background-size: 20px 20px;
background-position: 0 0, 0 10px, 10px -10px, -10px 0px;
```

### Racing Stripe Gradient
**Usage:** Headers, dividers, accent bars
```css
background: linear-gradient(90deg, #0066FF 0%, #FFB300 100%);
height: 4px;
```

### Speed Lines
**Usage:** Hover states, loading animations
```css
/* Diagonal lines suggesting motion */
background: repeating-linear-gradient(
  45deg,
  transparent,
  transparent 10px,
  rgba(0, 102, 255, 0.1) 10px,
  rgba(0, 102, 255, 0.1) 20px
);
```

### Paw Print Watermark
**Usage:** Empty states, dog-specific sections
```css
/* Subtle paw prints in background */
opacity: 0.05;
color: #8B6F47;
position: absolute;
```

---

## 🚀 Animation & Motion

### Micro-interactions

#### **Button Hover**
```css
transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
transform: translateY(-2px);
box-shadow: 0 8px 16px rgba(0, 102, 255, 0.3);
```

#### **Card Hover**
```css
transition: all 0.3s ease;
transform: translateY(-4px) scale(1.02);
box-shadow: 0 12px 32px rgba(0, 102, 255, 0.2);
border-color: #0066FF;
```

#### **Racing Number Flip** (On update)
```css
animation: flip 0.6s ease-in-out;
@keyframes flip {
  0% { transform: rotateX(0deg); }
  50% { transform: rotateX(90deg); }
  100% { transform: rotateX(0deg); }
}
```

#### **Checkered Flag Wave** (Loading)
```css
animation: wave 1.5s ease-in-out infinite;
@keyframes wave {
  0%, 100% { transform: rotate(-5deg); }
  50% { transform: rotate(5deg); }
}
```

#### **Speed Burst** (Success action)
```css
/* Radial expansion with speed lines */
animation: speedBurst 0.6s ease-out;
@keyframes speedBurst {
  0% { 
    transform: scale(0);
    opacity: 1;
  }
  100% { 
    transform: scale(2);
    opacity: 0;
  }
}
```

### Page Transitions
- **Duration:** 300ms
- **Easing:** `cubic-bezier(0.4, 0, 0.2, 1)`
- **Effect:** Slide up with fade (20px vertical offset)

---

## ♿ Accessibility Standards

### Color Contrast
- **WCAG AA Compliant:** All text/background combinations meet 4.5:1 ratio
- **High contrast mode:** Automatic border addition for interactive elements
- **Color blindness:** Never use color alone for information (add icons/text)

### Focus States
```css
outline: 3px solid #FFB300; /* Champion gold */
outline-offset: 2px;
border-radius: 4px;
```

### Keyboard Navigation
- **Tab order:** Logical flow (header → main → sidebar)
- **Skip links:** "Skip to main content" hidden until focused
- **Escape key:** Closes modals, cancels actions

### Screen Reader Support
- **Alt text:** All dog photos with breed/name
- **ARIA labels:** "Delete team Storm Runners" not just "Delete"
- **Live regions:** Race results, status updates announced

### Motion Preferences
```css
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## 📱 Responsive Design

### Mobile (320px - 767px)
- **Navigation:** Hamburger menu (checkered flag icon)
- **Cards:** Single column, full width
- **Typography:** Reduce hero to 48px, H1 to 32px
- **Spacing:** Reduce from lg (24px) to md (16px)
- **Touch targets:** Minimum 44x44px

### Tablet (768px - 1023px)
- **Cards:** 2-column grid
- **Navigation:** Horizontal with icons
- **Typography:** Slightly reduced (hero 60px)

### Desktop (1024px+)
- **Cards:** 3-column grid (teams), 4-column (dogs)
- **Navigation:** Full horizontal menu
- **Sidebar:** Optional quick actions panel

---

## 🎨 Design Rationale

### Why Racing + Dogs Works

**1. Shared Values:**
- **Speed:** Both dogs and racing celebrate velocity and agility
- **Competition:** Head-to-head racing, leaderboards, championships
- **Precision:** Timing, records, performance tracking
- **Team Spirit:** Handler-dog bond mirrors pit crew dynamics

**2. Visual Synergy:**
- **Bold Colors:** Racing uses high-contrast, energetic palettes (blue/gold)
- **Dynamic Shapes:** Angular racing stripes + organic dog silhouettes create balance
- **Numbers:** Racing culture loves big numbers (car numbers, lap times) → dog IDs, stats
- **Patterns:** Checkered flags are instantly recognizable, tie directly to racing

**3. Emotional Connection:**
- **Racing:** Excitement, adrenaline, achievement
- **Dogs:** Loyalty, companionship, joy
- **Together:** A professional system that's also approachable and fun

**4. Differentiation:**
- **Unique positioning:** Not generic pet management or plain sports app
- **Memorable brand:** Checkered patterns + paw prints = instant recognition
- **Target audience:** Appeals to both serious competitors and casual enthusiasts

### Design System Benefits

**For Users:**
- **Clear hierarchy:** Racing-inspired typography makes scanning easy
- **Intuitive actions:** Color-coded buttons (blue = primary, gold = special, red = danger)
- **Joyful experience:** Playful patterns without sacrificing professionalism

**For Developers:**
- **Token system:** Reusable colors, spacing, typography
- **Component library:** Pre-built cards, buttons, layouts
- **Scalable:** Easy to add new dog breeds, race types, features

**For Business:**
- **Brand identity:** Instantly recognizable visual language
- **Market fit:** Appeals to dog racing community aesthetics
- **Extensibility:** Pattern library supports future features (merchandise, social features)

---

## 🛠️ Implementation Roadmap

### Phase 1: Foundation (Week 1-2)
- [ ] Install design tokens (CSS variables or Tailwind config)
- [ ] Import fonts (Inter, Bebas Neue)
- [ ] Create color palette classes
- [ ] Set up spacing/sizing utilities

### Phase 2: Core Components (Week 3-4)
- [ ] Button component (all variants)
- [ ] Card component (team, dog, owner)
- [ ] Navigation header
- [ ] Modal/dialog system
- [ ] Icon library integration

### Phase 3: Page Layouts (Week 5-6)
- [ ] Dashboard grid layout
- [ ] Dog profile page
- [ ] Team management page
- [ ] Responsive breakpoints
- [ ] Loading states

### Phase 4: Polish (Week 7-8)
- [ ] Animations & micro-interactions
- [ ] Accessibility audit
- [ ] Performance optimization
- [ ] Documentation (Storybook?)
- [ ] Design QA

---

## 📚 Resources & References

### Design Tools
- **Mockups:** Figma (recommended), Sketch, Adobe XD
- **Icons:** Heroicons, Font Awesome, Custom SVG set
- **Illustrations:** Undraw, Humaaans (customize with brand colors)

### Inspiration Sources
- **Racing Apps:** Formula 1 app, iRacing, Gran Turismo
- **Dog Apps:** Rover, BringFido (for warmth and approachability)
- **Sports:** ESPN, TheScore (stats presentation)
- **Color Psychology:** Racing blues (BMW, Ford), gold (championships)

### Fonts
- **Inter:** https://fonts.google.com/specimen/Inter
- **Bebas Neue:** https://fonts.google.com/specimen/Bebas+Neue

### Code Examples
```html
<!-- Example: Racing stripe divider -->
<div style="
  height: 4px;
  background: linear-gradient(90deg, #0066FF 0%, #FFB300 100%);
  margin: 32px 0;
"></div>

<!-- Example: Velocity Blue button -->
<button style="
  background: #0066FF;
  color: white;
  padding: 12px 24px;
  border-radius: 8px;
  font-family: Inter;
  font-size: 14px;
  font-weight: 600;
  letter-spacing: 0.5px;
  border: none;
  cursor: pointer;
  transition: all 0.2s ease;
">
  Add Team
</button>

<!-- Example: Checkered pattern (CSS) -->
<style>
.checkered-divider {
  height: 40px;
  background-image: 
    linear-gradient(45deg, #1A1A1A 25%, transparent 25%),
    linear-gradient(-45deg, #1A1A1A 25%, transparent 25%),
    linear-gradient(45deg, transparent 75%, #1A1A1A 75%),
    linear-gradient(-45deg, transparent 75%, #1A1A1A 75%);
  background-size: 20px 20px;
  background-position: 0 0, 0 10px, 10px -10px, -10px 0px;
}
</style>
```

---

## 🏁 Next Steps

**For Design Team:**
1. Create Figma file with full component library
2. Design 3-5 key user flows (onboarding, add team, view race results)
3. User testing with dog racing community

**For Development Team:**
1. Review color palette and typography specs
2. Decide on implementation: Tailwind CSS vs CSS-in-JS vs CSS modules
3. Create shared component library (Storybook?)
4. Implement responsive breakpoints

**For Product Team:**
1. Validate brand direction with stakeholders
2. Plan marketing materials using design system
3. Consider merchandise (t-shirts with checkered paw prints?)

---

**Design System Owner:** Nova, Design Lead  
**Version:** 1.0  
**Last Updated:** 2024-01-15  
**Status:** Ready for Implementation 🏁

---

*"Where speed meets loyalty, champions are made."*  
— DogTeams Design Philosophy
