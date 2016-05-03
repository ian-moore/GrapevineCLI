namespace GrapevineCLI

module Items = 
    open System
    open Microsoft.Xrm.Sdk
    open Chessie.ErrorHandling
    open GrapevineCLI.CommandOptions
    open GrapevineCLI.CommonFuncs
    
    type Item = {Id:Guid; Number:int; Name:string; Project:string; Feature:string; DueDate:DateTime; Details:string; EstimatedHours:float}

    let private createItemFromEntity (entity:Entity) = 
        entity |> Trial.Catch(fun e -> 
            { Id = e.Id;
              Number = e.GetAttributeValue<int>("sonoma_itemnumber");
              Name = e.GetAttributeValue<string>("sonoma_name");
              Project = e.GetAttributeValue<EntityReference>("sonoma_projectid").Name;
              Feature = if e.Contains("sonoma_storyid") then e.GetAttributeValue<EntityReference>("sonoma_storyid").Name else "N/A";
              DueDate = e.GetAttributeValue<DateTime>("sonoma_developerduedate");
              Details = e.GetAttributeValue<string>("sonoma_description");
              EstimatedHours = e.GetAttributeValue<float>("sonoma_estdevhours"); })

    let retrieveItems filterXml =
        let query = sprintf "
            <fetch>
                <entity name='sonoma_item'>
                    <all-attributes/>
                    <order attribute='sonoma_itemnumber' descending='true' />
                    %s
                </entity>
            </fetch>" filterXml
        trial {
            let! entities = OrgService.fetchRecords query
            return! Seq.map createItemFromEntity entities |> collect
        }

    let private mapTableDisplayData items =
        let truncate25 = truncateEllipsis 25
        let truncate45 = truncateEllipsis 45
        Seq.map (fun i -> { i with Project = (truncate25 i.Project); Name = (truncate45 i.Name) }) items

    let private printItemRow items =
        Seq.map (fun i -> sprintf " %-6i | %-25s | %-45s | %s \n" i.Number i.Project i.Name <| datestring i.DueDate) items

    let getItemList () =
        let tableHeader = sprintf " Item # | %-25s | %-45s | Due Date\n" "Project" "Name"
        let filterXml = "
            <filter>
                <condition attribute='statecode' operator='eq' value='0' />
                <condition attribute='ownerid' operator='eq-userid' />
            </filter>"

        let buildOutput = mapTableDisplayData >> printItemRow >> Seq.append [tableHeader] >> String.concat ""
        trial {
            let! items = retrieveItems filterXml
            return buildOutput items
        }

    let retrieveSingleItem itemNo =
        trial {
            let! items = retrieveItems <| sprintf "<filter><condition attribute='sonoma_itemnumber' operator='eq' value='%i' /></filter>" itemNo
            match Seq.tryHead items with
            | Some i -> return i 
            | None -> return! fail (exn "Item not found.")
        }

    let getItemDetails itemNo =
        trial {
            let! i = retrieveSingleItem itemNo 
            return [sprintf "| #%d %s\n" i.Number i.Name;
                    sprintf "| %s (Feature: %s)\n" i.Project i.Feature;
                    sprintf "| Due %s | Estimated Hours: %A\n" (datestring i.DueDate) i.EstimatedHours
                    "| Details:\n\n"; i.Details; "\n"] 
                    |> String.concat ""
        }

    let openItemPage itemNo =
        let baseUrl = "https://crm.sonomapartners.com/Grapevine"
        trial {
            let! i = retrieveSingleItem itemNo
            do! sprintf "%s/main.aspx?etn=sonoma_item&id=%A&pagetype=entityrecord" baseUrl i.Id
                |> Trial.Catch(fun f -> System.Diagnostics.Process.Start(f) |> ignore)
            return "Item opened."
        }

    let completeItem itemNo =
        let entity = new Entity("sonoma_item")
        entity.Attributes.Add("statecode", new OptionSetValue(1));
        entity.Attributes.Add("statuscode", new OptionSetValue(2));
        trial {
            let! item = retrieveSingleItem itemNo
            entity.Id <- item.Id
            do! OrgService.updateRecord entity
            return sprintf "Item #%i has been marked completed." itemNo
        }

    let appProcess options =
        match options with
        | o when o.showMyItems -> getItemList ()
        | o when o.itemToDisplay > 0 -> getItemDetails o.itemToDisplay
        | o when o.itemToOpen > 0 -> openItemPage o.itemToOpen
        | o when o.itemToComplete > 0 -> completeItem o.itemToComplete
        | _ -> fail (exn "Unknown option for items command.")