namespace GrapevineCLI

module CommandOptions =
    open CommandLine

    [<Verb("items", HelpText="View, assign, or complete items.")>]
    type ItemOptions = {
        [<Option('d', "details", HelpText="View item details by item number.")>] itemToDisplay:int
        [<Option('l', "list", HelpText="Show list of my open items.")>] showMyItems:bool
        [<Option('o', "open", HelpText="Open item page in web browser.")>] itemToOpen:int
        [<Option('c', "complete", HelpText="Mark an item as completed.")>] itemToComplete:int
    }

    [<Verb("config", HelpText="View or edit configuration settings.")>]
    type ConfigOptions = {
        [<Option('l', "list", HelpText="Show list of current configuration settings.")>] showConfigList:bool
        [<Option('c', "connectionstring", HelpText="Set the connection string.")>] connectionString:string
    }