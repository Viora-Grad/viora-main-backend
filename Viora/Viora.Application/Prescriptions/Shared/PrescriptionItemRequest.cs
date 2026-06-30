namespace Viora.Application.Prescriptions.Shared;

public class PrescriptionItemDTO
{

    public string Name { get; set; }
    public string? Note { get; set; }
    public string Dose { get; set; }
    public int Frequence { get; set; }
    public int Duration { get; set; }

    public PrescriptionItemDTO(string name, string? note, string dose, int frequence, int duration)
    {
        Name = name;
        Note = note;
        Dose = dose;
        Frequence = frequence;
        Duration = duration;
    }

}
