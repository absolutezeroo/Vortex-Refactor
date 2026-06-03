namespace Vortex.Habbo.Avatar.Pets;

/// @see com.sulake.habbo.avatar.pets.PetCustomPart
public class PetCustomPart(int layerId, int partId, int paletteId)
{
    public int LayerId { get; set; } = layerId;
    public int PartId { get; set; } = partId;
    public int PaletteId { get; set; } = paletteId;
}
