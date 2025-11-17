//test CQC for QualIQon - starting with getting accses of data stored in the ARC 

#r "nuget: ARCtrl.NET, 2.0.2"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.1"

open System
open ARCtrl
open ARCtrl.NET
open ARCtrl.QueryModel

let path = @"/home/paulinehans/Dokumente/TestARCForQualIQon"

let arc = ARC.load path
arc.MakeDataFilesAbsolute()
arc.DataContextMapping()

  
let getData = arc.Assays.[0].Tables.[0]
printfn"%A"getData


let getIdentifier = arc.AssayIdentifiers
printfn "%A" getIdentifier

//ASSAY CHECK

//check isa file 
//check for MeasurementType Proteomics/proteomics 
let getMesurementType = arc.Assays.[0].MeasurementType.Value
let checkProteomics = 
    if getMesurementType.NameText.ToLower().Contains("proteomics") then printfn "Proteomics found"
    else failwith"no Proteomics found in Measurement type"
printfn "%A" checkProteomics

//check for Technologytype Mass Spectrometry 
let getTechnologyType = arc.Assays.[0].TechnologyType.Value
printfn "%A" getTechnologyType
let checkMassSpectrometry  = 
    if getTechnologyType.NameText.ToLower().Contains("mass spectrometry") then printfn "Mass Spectrometry as indicator for Proteomics found"
    else failwith"no Mass spectrometry as indicator for Proteomics found" 
checkMassSpectrometry

//check Assay tables 
//output files 

let outputFilesWiff = arc.GetAssay("dilutionSeriesChlamy_ASSAY").LastData
let verify : bool =
    outputFilesWiff
    |> List.exists (fun x -> x.Name.Contains("wiff"))
verify

//check for Digestion 
//Parameter Digestion 
let getAllParameters (table:ArcTable) =
    table.Headers
    |> Seq.choose (fun x ->
        match x with
        | CompositeHeader.Parameter p -> Some p
        | _ -> None )
    |> Seq.toList
let dataDigestion = getAllParameters getData 
let checkParameterDisgest = dataDigestion |> List.exists (fun x -> x.Name.Value.Contains("Digestion"))
checkParameterDisgest

//checkForTrypsin 
let trypsinTerm (header: CompositeHeader) =
    match header with 
    | CompositeHeader.Parameter oa -> Some oa 
    | _ -> None

let getOntologyListByHeaderOntology (table : ArcTable) (ontologyName : string) =
            let isOntologyHeader (header : CompositeHeader)=
                    match trypsinTerm header with
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

let exe = getOntologyListByHeaderOntology getData "Digestion"
let validateTrypsin = exe |> List.exists (fun x -> x.Name.Value.Contains("Trypsin"))
validateTrypsin
let validateLysC = exe |> List.exists (fun x -> x.Name.Value.Contains("Lys-C"))
validateLysC