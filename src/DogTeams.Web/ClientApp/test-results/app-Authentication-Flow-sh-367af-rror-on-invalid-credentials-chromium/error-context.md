# Page snapshot

```yaml
- generic [ref=e3]:
  - heading "Dog Teams" [level=1] [ref=e4]
  - heading "Sign in" [level=2] [ref=e5]
  - generic [ref=e6]:
    - generic [ref=e7]:
      - text: Email
      - textbox "Email" [ref=e8]
    - generic [ref=e9]:
      - text: Password
      - textbox "Password" [ref=e10]
    - button "Sign in" [ref=e11] [cursor=pointer]
  - paragraph [ref=e12]:
    - text: Don't have an account?
    - link "Register" [ref=e13] [cursor=pointer]:
      - /url: /register
```