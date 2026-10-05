using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Manufacturing;

public sealed record CabinetManufacturingSpec(
    Length GrooveWidth,
    Length GrooveDepth,
    Length GrooveOffsetFromRear,
    Length BackPartWidth,
    Length BackPartHeight);
