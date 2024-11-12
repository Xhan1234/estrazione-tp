using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebApp.Models
{
public class TaskData
{
    public string? Rete { get; set; }
    public string? NomeRete { get; set; }
    public string? Pdv { get; set; }
    public long IdTask { get; set; }
    public DateTime DataCreazioneTask { get; set; }
    public DateTime? DataConsegna { get; set; }
    public string? StatoBRT { get; set; }
    public string? LetteraDiVettura { get; set; }
    public int QuantitaOBU { get; set; }
    public string? TipologiaOBU { get; set; }
    public string? StatoTask { get; set; }
    public string? TipologiaTask { get; set; }
    public string? PrimoApprovvigionamento { get; set; }
}


}
