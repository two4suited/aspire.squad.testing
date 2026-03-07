# Page snapshot

```yaml
- generic [ref=e3]:
  - heading "Dog Teams" [level=1] [ref=e4]
  - heading "Create account" [level=2] [ref=e5]
  - paragraph [ref=e6]: Failed to fetch
  - generic [ref=e7]:
    - generic [ref=e8]:
      - text: Name
      - textbox "Name" [ref=e9]: Test User
    - generic [ref=e10]:
      - text: Email
      - textbox "Email" [ref=e11]: test+1772896043498@example.com
    - generic [ref=e12]:
      - text: Password
      - textbox "Password" [ref=e13]: TestPassword123!
    - button "Create account" [ref=e14] [cursor=pointer]
  - paragraph [ref=e15]:
    - text: Already have an account?
    - link "Sign in" [ref=e16] [cursor=pointer]:
      - /url: /login
```