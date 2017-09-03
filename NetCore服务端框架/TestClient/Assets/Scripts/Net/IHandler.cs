using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHandler {
    void MessageReceive(MessageModel model);
    void Set(object value);
    void Do();
    IHandler Clone();
}
