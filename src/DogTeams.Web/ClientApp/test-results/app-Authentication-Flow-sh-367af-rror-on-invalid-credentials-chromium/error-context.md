# Page snapshot

```yaml
- generic [ref=e3]:
  - heading "Dog Teams" [level=1] [ref=e4]
  - heading "Sign in" [level=2] [ref=e5]
  - paragraph [ref=e6]: Failed to fetch
  - generic [ref=e7]:
    - generic [ref=e8]:
      - text: Email
      - textbox "Email" [ref=e9]: nonexistent@example.com
    - generic [ref=e10]:
      - text: Password
      - textbox "Password" [ref=e11]: wrongpassword
    - button "Sign in" [ref=e12] [cursor=pointer]
  - paragraph [ref=e13]:
    - text: Don't have an account?
    - link "Register" [ref=e14] [cursor=pointer]:
      - /url: /register
```