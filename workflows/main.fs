let path = @"/home/paulinehans/Dokumente/TestARCForQualIQon"

let arc = ARC.load path
arc.MakeDataFilesAbsolute()

let accsessAssays = arc.Assays.[0].Tables.[0]
printfn"%A"accsessAssays


//extract Parameters out of Assays > Table 1
let getAllParameters (table:ArcTable) =
    table.Headers
    |> Seq.choose (fun x ->
        match x with
        | CompositeHeader.Parameter p -> Some p
        | _ -> None )
    |> Seq.toList

let get =  accsessAssays |> getAllParameters
printfn "%A" get


let whatDefinesAProteomicARC = 
    let definitionList = [
        "modification parameters"
    ]
    let studies = arc.Studies.[0].Tables.[0] //name von Tabel printen
    let getAllParameters (table:ArcTable) =
        table.Headers
        |> Seq.choose (fun x ->
            match x with
            | CompositeHeader.Parameter p -> Some p
            | _ -> None )
        |> Seq.toList
    let checkforProteomics (x : string) = 
        if x = definitionList then getAllParameters
        else x |> failwith "No identification parameter, which indicates its not a proteomics related ARC"
    checkforProteomics
whatDefinesAProteomicARC

