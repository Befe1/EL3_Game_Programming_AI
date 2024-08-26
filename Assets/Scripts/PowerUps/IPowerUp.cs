using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPowerUp 
{
    bool IsActive { get;set; }
    bool IsReady { get; set; }

    public void Activate();
    public void Deactivate();


}
