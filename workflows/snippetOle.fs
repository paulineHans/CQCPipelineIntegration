let path = @"/home/paulinehans/Dokumente/TestARCForQualIQon"

let arc = ARC.load path
arc.MakeDataFilesAbsolute()
let blub = arc.Assays.[0].Tables.[0]
printfn"%A"blub

// extrahiert alle characteristics 
let getAllCharacteristics (table:ArcTable) =
        table.Headers
        |> Seq.choose (fun x ->
            match x with
            | CompositeHeader.Characteristic c -> Some c
            | _ -> None )
        |> Seq.toList
 
let getAllParameters (table:ArcTable) =
    table.Headers
    |> Seq.choose (fun x ->
        match x with
        | CompositeHeader.Parameter p -> Some p
        | _ -> None )
    |> Seq.toList
 
let getAllFactors (table:ArcTable) =
    table.Headers
    |> Seq.choose (fun x ->
        match x with
        | CompositeHeader.Factor f -> Some f
        | _ -> None )
    |> Seq.toList

//CompoHeader = Parameter Disgetion
let tryTerm (header : CompositeHeader) =
        match header with
        | CompositeHeader.Characteristic oa
        | CompositeHeader.Component oa
        | CompositeHeader.Parameter oa
        | CompositeHeader.Factor oa -> Some oa
        | _ -> None

//alle Zellen, werte für bestimmter header 
let getOntologyListByHeaderOntology (table : ArcTable) (ontologyName : string) =
            let isOntologyHeader (header : CompositeHeader)=
                    match tryTerm header with
                    | Some oa -> oa.NameText = ontologyName
                    | None -> false     
            let colOption = ArcTable.tryGetColumnByHeaderBy isOntologyHeader table
            match colOption with
            | Some col ->  
                    col.Cells
                    |> Seq.map (fun (cell:CompositeCell) -> cell.AsTerm)
                    |> Seq.distinct
                    |> List.ofSeq
            | None -> []



let getMeasurementDevice (table:ArcTable) =
        getOntologyListByHeaderOntology table "Digestion"

let oaList =
    getMeasurementDevice arc.Assays.[0].Tables.[0]
 
oaList
|> List.tryPick (fun oa -> oa.Name)








//Code Snippet Caro
// let sampleNameColumn : string = "test"  
// let getSample (fN) =
//     arc.PreviousParametersOf(fN).[sampleNameColumn].ValueText 

// let condition1 : string = "test2"

// let getCondition1 (fN) =
//     lastData
//     |> Seq.filter (fun x -> getSample x = fN)
//     |> Seq.map (fun x ->
//         match arc.PreviousValuesOf(x).[condition1].TryValueText with
//         | Some x -> x
//         | None -> ""
//     )
//     |> Seq.head

// let columnToFilter : string = "test1"
// let getFilterColumn (fN) =
//     lastData
//     |> Seq.filter (fun x -> getSample x = fN)
//     |> Seq.map (fun x ->
//         match arc.PreviousValuesOf(x).[columnToFilter].TryValueText with
//         | Some x -> x
//         | None -> ""
//     )
//     |> Seq.head