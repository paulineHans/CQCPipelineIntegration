
//test CQC for QualIQon - starting with getting accses of data stored in the ARC 

#r "nuget: ARCtrl.NET, 2.0.2"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.1"

open System
open ARCtrl.FileSystem
open ARCtrl
open ARCtrl.NET
open ARCtrl.QueryModel
open System.IO 
open System

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
printfn "%A" outputFilesWiff
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
let allParameters = getAllParameters getData 
let checkParameterDisgest = allParameters |> List.exists (fun x -> x.Name.Value.Contains("Digestion"))
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

//check for anything with labeling 
let searchForLabeling = allParameters |> List.exists (fun x -> x.Name.Value.Contains("labeling"))
searchForLabeling

let exe1 = getOntologyListByHeaderOntology getData "Isotope labeling"
let validate15N = exe1 |> List.exists (fun x -> x.Name.Value.Contains("15N"))
validate15N




let run = arc.Runs
// CHECK RUNS 
let runs = arc.Runs.[0].Tables.[0]
let checkForMzMLFiles = runs.OutputNames |> List.exists(fun x -> x.Contains("mzML"))
checkForMzMLFiles

let checkForMzLightFiles = 
    let data = arc.Runs.[0].Tables.[1]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("mzlite"))
    validation
checkForMzLightFiles

let checkForPSMFiles = 
    let data = arc.Runs.[0].Tables.[2]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("psm"))
    validation
checkForPSMFiles 

let checkForPSMSFiles = 
    let data = arc.Runs.[0].Tables.[3]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("qpsm"))
    validation
checkForPSMSFiles 

let checkForQuantFiles = 
    let data = arc.Runs.[0].Tables.[4]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("quant"))
    validation
checkForQuantFiles 

let checkForProtFiles = 
    let data = arc.Runs.[0].Tables.[5]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("prot"))
    validation
checkForProtFiles 


//IDEE = such nach files im ARC z.B. runs -> psmstats falls vorhanden dann suchen wo dieses file in den
//metadaten vorhanden ist und parameter geben z.B. labeling 

let rec searchFiles (directoryName: string) (fileName: string) : string[] =
    // Files im aktuellen Verzeichnis
    let currentFiles : string[] =
        Directory.GetFiles(path = directoryName, searchPattern = fileName)

    // alle Subdirs
    let subDirectories : string[] =
        Directory.GetDirectories(path = directoryName)

    // rekursiv in Subdirs suchen
    let subDirFiles : string[] =
        subDirectories
        |> Array.collect (fun subDir -> searchFiles subDir fileName)

    // beides zusammenkleben
    Array.append currentFiles subDirFiles


let searchForPattern : string list = [
    "*.psm"
    "*.qpsm"
    "*.quant"
    "*.prot"
    "*.tsv"
    "*.txt"
]

let accessFiles (directoryName: string) : string[] =
    let directoryPath = "./runs" + directoryName

    searchForPattern
    |> List.map (fun pattern -> searchFiles directoryPath pattern) 
    |> List.toArray                                               
    |> Array.concat                                               

let execution = accessFiles "/dilutionSeriesChlamy_RUNS"
let validaaation = 
    if Array.isEmpty execution then printfn"no files detected"
    else printfn "files detected"
validaaation

//rekursive FUnktion um checkt ok sind die Files da, jetzt müssen wir die Files in den metadaten suchen also table übergreifend 


    
