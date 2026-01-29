using Febucci.UI;
using UnityEngine;

public interface IDialogueSpeaker
{
    CustomerID SpeakerID { get; set; } // String yerine Enum

    Transform LookAtPoint { get; set;  }
}