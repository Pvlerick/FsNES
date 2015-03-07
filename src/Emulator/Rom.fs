namespace FsNes.Emulator

    open System.IO
    
    module Constants =
        let HeaderSize = 16
        let PrgBankSize = 16 * 1024 //16K
        let ChrBankSize = 8 * 1024 //8K
        let TitleDataSize = 128

    //Represents a rom in the iNES format
    type public Rom(path) = 
        let data = File.ReadAllBytes(path)
        let header = data.[0..Constants.HeaderSize - 1]
        let prgBanksCount = System.BitConverter.ToInt32([| header.[4]; byte(0); byte(0); byte(0) |], 0)
        let chrBanksCount = System.BitConverter.ToInt32([| header.[5]; byte(0); byte(0); byte(0) |], 0)

        let prgBanksStartOffset = Constants.HeaderSize - 1
        let prgBanks = [| for i in 0 .. prgBanksCount - 1 -> data.[(prgBanksStartOffset + (i * Constants.PrgBankSize))..(prgBanksStartOffset + (i * Constants.PrgBankSize) + Constants.PrgBankSize)] |]
            
        let chrBanksStartOffset = (Constants.HeaderSize - 1) + (prgBanks.Length * Constants.PrgBankSize)
        let chrBanks = [| for i in 0 .. chrBanksCount - 1 -> data.[(chrBanksStartOffset + (i * Constants.ChrBankSize))..(chrBanksStartOffset + (i * Constants.ChrBankSize) + Constants.ChrBankSize)] |]

        let titleData = //Title data is optional - if present, the last 128 bytes from the rom file
            if not(data.Length = (Constants.HeaderSize + (prgBanks.Length * Constants.PrgBankSize) + (chrBanks.Length * Constants.ChrBankSize)))
                then System.BitConverter.ToString(data.[(data.Length - Constants.TitleDataSize)..(data.Length - 1)])
                else ""

        do
            () //Nothing to do here

        member this.Header with get() = header
        member this.PrgBanks with get() = prgBanks
        member this.ChrBanks with get() = chrBanks
        member this.TitleData with get() = titleData
        member this.RawData with get() = data

        //Entry point for nes rom: C000