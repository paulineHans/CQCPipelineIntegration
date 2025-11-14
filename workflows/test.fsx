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


//check tables 
//input files 

let outputFilesWiff = arc.GetAssay("dilutionSeriesChlamy_ASSAY").LastData

let verify : bool =
    outputFilesWiff
    |> List.exists (fun x -> x.Name.Contains("wiff"))
verify















 