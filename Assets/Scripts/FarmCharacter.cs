using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FarmCharacter : Character
{
    public enum Team { RED, BLUE }
    public enum Type { FARMER, WOODCUTTER }

    public Team team;
    public Type type;

    public bool hasLog;
    public bool hasCarrot;
    
    public UnityEvent<bool> GotLog;
    public UnityEvent<bool> GotCarrot;


}
