namespace GrapevineCLI.Tests

open NUnit.Framework
open FsCheck

module ``Common Function Tests`` =
    open GrapevineCLI.CommonFuncs

    [<Test>]
    let ``truncate overflow replaced by arg`` () =
        let result = truncateWithOverflow "..." 15 "This text is too long"
        Assert.That(result, Is.EqualTo("This text is..."))

    [<Test>]
    let ``text under truncate length not truncated`` () =
        let result = truncateWithOverflow "-" 20 "Hello world!"
        Assert.That(result, Is.EqualTo("Hello world!"))

    [<Test>]
    let ``empty overflow string doesn't replace`` () =
        let result = truncateWithOverflow "" 10 "Hello world!"
        Assert.That(result, Is.EqualTo("Hello worl"))