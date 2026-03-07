# Design Initiative: Dog-Themed Branding for DogTeams

**Date:** 2026-03-07  
**Designer:** Nova 🎨  
**Status:** Ready for kickoff (pending E2E validation)

## Vision

Transform DogTeams into a warm, playful, dog-themed application that celebrates the sport of flyball canine sports. The interface should evoke trust, fun, and community—matching the enthusiasm of dog owners and handlers.

## Design System Deliverables

### 1. Color Palette
**Primary Colors** (warm, dog-inspired):
- `#D4A574` — Golden Retriever (warm gold) — Primary CTA & accents
- `#8B6F47` — Chocolate Labrador (rich brown) — Secondary & depth
- `#F5E6D3` — Cream/Light fur (backgrounds & subtle overlays)
- `#2C3E50` — Deep slate (text, serious elements)

**Accent Colors** (breed variations):
- `#FF6B35` — Corgi Red (alerts, warnings)
- `#4ECDC4` — Border Collie Blue (success, positive actions)
- `#95E1D3` — Mint (info, secondary highlights)

**Utility**:
- `#FFFFFF` — Pure white (text, contrast)
- `#F0F0F0` — Off-white (backgrounds)
- `#E0E0E0` — Light gray (borders, dividers)

### 2. Typography

**Headings:**
- Font: `Inter`, `Poppins`, or system sans-serif
- Weight: 700 (bold) for h1/h2
- Style: Friendly, rounded terminals

**Body:**
- Font: `Inter` or system sans-serif
- Size: 14-16px for readability
- Line-height: 1.6 for comfort
- Weight: 400 (regular)

**Accents:**
- Paw print symbols (🐾) for emphasis
- Dog breed icons for navigation sections

### 3. Component Design

**Buttons:**
- Shape: Rounded corners (16px border-radius)
- Primary: Gradient from gold to brown
- Hover: Subtle shadow lift (transform: translateY(-2px))
- State: Loading spinner (bone icon)

**Cards:**
- Paw print border accent (top-left corner)
- Shadow: Soft (0 4px 12px rgba(0,0,0,0.1))
- Hover: Slight scale (1.02x)

**Navigation:**
- Dog breed icons for main sections (Teams, Dogs, Owners, Clubs)
- Sidebar with paw trail animation on active

**Forms:**
- Input labels in warm brown
- Focus state: Gold border + glow
- Validation: Paw print checkmark ✓

**Icons:**
- Custom paw print variations for actions
- Dog breed silhouettes for categories
- Bone icons for loading/connecting states

### 4. Accessibility (WCAG 2.1 AA+)

- **Color Contrast:** All text ≥ 4.5:1 ratio
- **Keyboard Navigation:** Full support (Tab, Enter, Escape)
- **Screen Reader:** Semantic HTML, aria labels
- **Motion:** Respects `prefers-reduced-motion`
- **Touch Targets:** Minimum 44x44px for clickable elements

### 5. Responsive Design

**Breakpoints:**
- Mobile: 320px - 639px
- Tablet: 640px - 1023px
- Desktop: 1024px+

**Mobile-First:**
- Single column layouts
- Touch-friendly spacing (minimum 16px padding)
- Optimized navigation (hamburger menu)

### 6. Design Tokens (Tailwind Config)

```js
// tailwind.config.ts additions
colors: {
  'dog-gold': '#D4A574',
  'dog-brown': '#8B6F47',
  'dog-cream': '#F5E6D3',
  'dog-slate': '#2C3E50',
  'dog-corgi-red': '#FF6B35',
  'dog-collie-blue': '#4ECDC4',
  'dog-mint': '#95E1D3',
}
fontSize: {
  'xs': '12px',
  'sm': '14px',
  'base': '16px',
  'lg': '18px',
  'xl': '20px',
}
```

## Implementation Roadmap

**Phase 1 (Week 1):** Core design system
- [ ] Create Figma/design file with components
- [ ] Document design tokens
- [ ] Create Tailwind configuration with dog theme
- [ ] Build component storybook

**Phase 2 (Week 2):** Frontend integration
- [ ] Naomi implements components using design tokens
- [ ] Apply dog theme to existing pages (Teams, Auth, Dashboard)
- [ ] Add paw print accents and dog breed icons
- [ ] Test a11y compliance

**Phase 3 (Week 3):** Polish & refinement
- [ ] Visual QA (Bobbie)
- [ ] Mobile responsiveness verification
- [ ] Animation fine-tuning (loading, hover states)
- [ ] Documentation & design handoff

## Collaboration

**With Naomi (Frontend):**
- Nova designs → Naomi implements
- Review component implementations
- Iterate on visual feedback

**With Bobbie (QA):**
- Visual regression testing
- Accessibility audits
- Responsive design verification
- Cross-browser testing

**With Holden (Lead):**
- Design direction approval
- Stakeholder feedback
- Scope adjustments

## Success Criteria

✅ Dog theme visually evident across all pages  
✅ WCAG 2.1 AA+ accessibility compliance  
✅ Mobile-responsive (tested on iOS/Android)  
✅ Consistent component library  
✅ Tailwind integration complete  
✅ Design tokens documented  

---

**Next Step:** Await E2E validation results, then kick off Phase 1 design work.
