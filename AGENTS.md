# Coding Guidelines

## C#

- Name every `CancellationToken` parameter and local variable `ct`.
- If any argument in a call is on a new line, put every argument on its own line, starting after the opening parenthesis.
- Use braces for every branch if an `if` has an `else` branch. Use braces around any `if` body that spans multiple lines.
- Use `IReadOnlyCollection<T>` for collections that do not need indexing or collection-specific methods.
- Do not use the `sealed` modifier.
- Separate adjacent methods with exactly one blank line, including expression-bodied methods. Follow the same rule for adjacent properties and events.
- Separate logical blocks within methods with blank lines.
- Do not add code comments. Use clear names for methods, types, and variables. Extract a step into a descriptively named method when it makes the code easier to read.
