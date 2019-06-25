# Guid-To-Base64-Example

I read this blog post https://www.stevejgordon.co.uk/using-high-performance-dotnetcore-csharp-techniques-to-base64-encode-a-guid. I love little things like this that you can make fast. Decided to play around with Span’s and String.Create. Ended up creating code that is ~40% faster than best code currently in the post.
