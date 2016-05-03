namespace GrapevineCLI

module OrgService =
    open Microsoft.Xrm.Sdk
    open Microsoft.Xrm.Sdk.Query
    open Microsoft.Xrm.Client
    open Microsoft.Xrm.Client.Services
    open Chessie.ErrorHandling

    let private parseConnectionString connectionString =
        Trial.Catch(fun s -> CrmConnection.Parse s) connectionString
    
    let private createServiceProxy () =
        trial {
            let! connectionString = Config.getConnectionString ()
            let! connection = parseConnectionString connectionString
            return new OrganizationService(connection)
        }

    let private retrieveMultiple fetchXml (service:OrganizationService) =
        new FetchExpression(fetchXml)
        |> Trial.Catch(fun query -> 
            service.RetrieveMultiple(query).Entities :> seq<Entity>)

    let fetchRecords fetchXml = 
        createServiceProxy () >>= retrieveMultiple fetchXml

    let updateRecord (entity:Entity) =
        trial {
            let! service = createServiceProxy () 
            return! entity |> Trial.Catch(fun e -> service.Update(e))
        }