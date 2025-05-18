using System;
using UnityEngine;

namespace RedGaint.Games.Core
{
    //Game Events
    public struct RequestGroupDestroyEvent
    {
        public CardGroup Group;
        public bool Immediate; 
    
        public static RequestGroupDestroyEvent Create(CardGroup group, bool immediate = false)
        {
            return new RequestGroupDestroyEvent {
                Group = group,
                Immediate = immediate
            };
        }
    }
    public struct RequestRearrangeCardEvent
    {
        public CardGroup Group;
        public bool Immediate; 
        public static RequestRearrangeCardEvent Create(CardGroup group, bool immediate = false)
        {
            return new RequestRearrangeCardEvent {
                Group = group,
                Immediate = immediate
            };
        }
    }

}