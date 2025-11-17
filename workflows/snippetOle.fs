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




    type WorkflowArgs =
        | [<Unique; AltCommandLine("-i")>] ARC_Path of string
        | [<Unique; AltCommandLine("-o")>] Output_Path of string
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | ARC_Path _     -> "Path to the input ARC folder"
                | Output_Path _  -> "Path to the output file folder"

    let parser = ArgumentParser.Create<WorkflowArgs>(programName = "01_Heatmap.fsx")
    let args = fsi.CommandLineArgs |> Array.skip 1
    let results : ParseResults<WorkflowArgs> = parser.ParseCommandLine(inputs = args, raiseOnUsage = false)

    if results.IsUsageRequested || (results.TryGetResult ARC_Path = None && results.TryGetResult Output_Path = None) then
        printfn "%s" (parser.PrintUsage())

    let arcPath =
        match results.TryGetResult ARC_Path with
        | Some p -> printfn "ARC path set to: %s" p; p
        | None ->
            let defaultPath = __SOURCE_DIRECTORY__ + "/../../"
            printfn "No ARC path given. Using default: %s" defaultPath
            defaultPath

    let outputPath =
        match results.TryGetResult Output_Path with
        | Some p -> printfn "Output path set to: %s" p; p
        | None ->
            let defaultPath = __SOURCE_DIRECTORY__ + "/../../runs/Proteomics/heatmap/"
            printfn "No output path given. Using default: %s" defaultPath
            defaultPath

// printfn "[INFO] ARC path argument: %s" Arguments.arcPath
// printfn "[INFO] Output path argument: %s" Arguments.outputPath
///////////////////////////
//// Prep paths
///////////////////////////
let outDir = Arguments.outputPath
Directory.CreateDirectory(outDir) |> ignore
let arcPath = Arguments.arcPath 
///////////////////////////
//// Accessing the ARC
///////////////////////////
let arc = ARC.load(arcPath)
arc.MakeDataFilesAbsolute()
arc.DataContextMapping()


///////////////////////////
//// Preparing meta data lookup
// printfn ("[DEBUG] Preparing meta data lookup")
///////////////////////////
let bioRepGroup = OntologyAnnotation.fromTermAnnotation("DPBO:1000183", name = "biological replicate group")
let timePoint = OntologyAnnotation.fromTermAnnotation("NFDI4PSO:0000034", name = "Time point")
let bioRep = OntologyAnnotation.fromTermAnnotation("NFDI4PSO:0000042", name = "Biological replicate")
let phase = OntologyAnnotation.fromTermAnnotation("NCIT:C25257", name = "Phase")
let protID = OntologyAnnotation.fromTermAnnotation("NCIT:C165059", name = "Protein Identifier")
let lfqIntensity = OntologyAnnotation.fromTermAnnotation("MS:1001902", name = "MaxQuant:LFQ intensity")

let getBioRepGroup (fN: QNode) =
    arc.PreviousCharacteristicsOf(fN).[bioRepGroup].ValueText 
let getTimePoint (fN: QNode) =
    arc.PreviousParametersOf(fN).[timePoint].ValueText
let getBioRep (fN: QNode) =
    arc.PreviousCharacteristicsOf(fN).[bioRep].ValueText
let getPhase (fN: QNode) =
    arc.PreviousParametersOf(fN).[phase].ValueText

///////////////////////////
//// Metadata-guided identification of samples and files to analyze 
// printfn ("[DEBUG] Metadata-guided identification of samples and files to analyze")
///////////////////////////
let dataToAnalyse = arc.GetAssay("Proteomics_Imputation").LastData

let availableGroups = 
    dataToAnalyse
    |> Seq.map (fun x -> getBioRepGroup x)
    |> Seq.distinct

let exp35, exp40 = Seq.item 0 availableGroups, Seq.item 1 availableGroups

let only35degreesRep1 = 
    dataToAnalyse
    |> Seq.filter (fun x -> getBioRepGroup x = exp35)
    |> Seq.filter (fun x -> getBioRep x = "1")

let filePath = 
    only35degreesRep1
    |> Seq.map (fun x -> x.FilePath)
    |> Seq.distinct
    |> Seq.head
 