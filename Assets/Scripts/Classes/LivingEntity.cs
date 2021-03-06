﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Threading;

[System.Serializable]
public class LivingEntity 
{
    private Player instance;

    private string name;
    private string character;
    private float moveSpeed;
    private float slowedSpeed;
    private float currSpeed;
    private float launchSpeed;
    private int maxHealth;
    private float currHealth;
    private int life;

    private bool stunned = false;
    private int stunIndex = 0;
    private bool slowed = false;
    private bool slowedOverTime = false;
    private int slowIndex = 0;
    private bool launching = false;
    private bool knockUped = false;
    private int knockUpIndex = 0;
    private float knockUpTime = 0;
    private bool knockBacked = false;
    private int knockBackIndex = 0;
    private bool dashing = false;
    private bool virgoDash = false;
    private bool invuln = false;

    private Attacks stunSpell;
    private Attacks slowSpell;
    private Attacks slowOverTimeSpell;
    private Attacks knockUpSpell;
    private Attacks knockBackSpell;

    Thread StunThread;
    Thread SlowOverTimeThread;
    Thread KnockUpThread;
    Thread KnockBackThread;
    Thread DashThread;

    public LivingEntity(GameObject playerObject, string name, string character, float moveSpeed, float slowedSpeed, int maxHealth, int lifes)
    {
        instance = playerObject.GetComponent<Player>();

        this.life = lifes;
        this.maxHealth = maxHealth;
        this.name = name;
        this.character = character;
        this.moveSpeed = moveSpeed;
        currSpeed = moveSpeed;
        this.slowedSpeed = slowedSpeed;
        this.currHealth = maxHealth;
        launchSpeed = moveSpeed;
    }

    #region Get & Set
    public float MoveSpeed { get { return currSpeed; } }
    public string Name { get { return name; } }
    public string Character { get { return character; } }
    public int Life { get { return life; } set { life = value; } }
    public float CurrHealth { get { return currHealth; } }
    public float MaxHealth { get { return maxHealth; } }

    public bool Stunned { get { return stunned; } }
    public bool Slowed
    {
        get
        {
            if (!slowed && !slowedOverTime) { return false; }
            else { return true;  }
        }
    }
    public bool KnockUped { get { return knockUped; } }
    public bool KnockBacked { get { return knockBacked; } }
    public bool Dashing { get { return dashing; } }
    public bool VirgoDash { get { return virgoDash; } set { virgoDash = value; } }
    public bool Invuln { get { return invuln; } set { invuln = value; } }
    public Attacks StunSpell { get { return stunSpell; } }
    public Attacks SlowSpell { get { return slowSpell; } }
    public Attacks SlowOverTimeSpell { get { return slowOverTimeSpell; } }
    public Attacks KnockUpSpell { get { return knockUpSpell; } }
    public Attacks KnockBackSpell { get { return knockBackSpell; } }
    #endregion

    #region Functions

    // heal Player
    public void Heal(float _ammount) {
        float newHealth = currHealth + _ammount;

        if(newHealth > maxHealth) { currHealth = maxHealth; }
        else { currHealth = newHealth; }
    }

    // damage Player
    public void GetDamage(float _ammount) {
        if (!stunned || !knockBacked || !knockUped) { instance._animator.SetTrigger("Damage"); }

        instance.ChangeToHitColor(instance.changebackTime);

        currHealth -= _ammount;

        CustomDebug.Log("<b>" + name + "</b> got <color=red>" + _ammount + " damage</color>, now has " + currHealth + " health","Damage");

        if (_ammount >= maxHealth || currHealth <= 0) { currHealth = 0; instance.Die(); }
    }

    #region Conditions
    #region apply condition
    public void ApplyStun(float _time, Attacks _spell)
    {
        stunSpell = _spell;
        StunThread = new Thread(() => Stun(_time));

        try { StunThread.Start(); }
        catch (ThreadStateException) { Debug.LogError("Error with StunThread Thread"); }
    }

    public void ApplySlow(bool _toggle, Attacks _spell)
    {
        if (_toggle)
        {
            if (currSpeed > slowedSpeed)
            {
                slowSpell = _spell;
                currSpeed = slowedSpeed;
                slowed = true;
                CustomDebug.Log("<b>" + name + "</b>s <color=magenta>Slow</color> start", "Condition");
            }
        }

        else
        {
            slowed = false;
            CustomDebug.Log("<b>" + name + "</b>s <color=magenta>Slow</color> stop", "Condition");

            if (!slowedOverTime) { currSpeed = moveSpeed; }
            if (launching) { currSpeed = launchSpeed; }
        }
    }

    public void ApplySlowOverTime(int _time, Attacks _spell)
    {
        slowOverTimeSpell = _spell;
        SlowOverTimeThread = new Thread(() => SlowOverTime(_time));

        try { SlowOverTimeThread.Start(); }
        catch (ThreadStateException) { Debug.LogError("Error with SlowOverTimeThread Thread"); }
    }

    public void ApplyLaunch(bool _toggel, float _speed)
    {
        if (_toggel)
        {
            launching = true;
            CustomDebug.Log("<b>" + name + "</b>s <color=magenta>Launch</color> start", "Condition");

            if (_speed < launchSpeed) { launchSpeed = _speed; }

            if (_speed < currSpeed) { currSpeed = _speed; }
        }

        else
        {
            launching = false;
            CustomDebug.Log("<b>" + name + "</b>s <color=magenta>Launch</color> stop", "Condition");

            launchSpeed = moveSpeed;

            if (slowed || slowedOverTime) { currSpeed = slowedSpeed; }
            else { currSpeed = moveSpeed; }
        }
    }

    public float ApplyKnockUp(float _height, Attacks _spell)
    {
        knockUpSpell = _spell;
        KnockUpThread = new Thread(() => KnockUp(_height));

        //instance._animator.SetTrigger("KnockUp");

        try { KnockUpThread.Start(); }
        catch (ThreadStateException) { Debug.LogError("Error with PlayerKnockUp Thread"); }

        return knockUpTime;
    }

    public void ApplyKnockBack(float _xDistance, float _time, Attacks _spell)
    {
        knockBackSpell = _spell;
        KnockBackThread = new Thread(() => KnockBack(_xDistance, _time));

        try { KnockBackThread.Start(); }
        catch (ThreadStateException) { Debug.LogError("Error with KnockBackThread Thread"); }
    }

    public void ApplyDash(float _xDistance, float _time)
    {
        DashThread = new Thread(() => Dash(_xDistance, _time));

        try { DashThread.Start(); }
        catch (ThreadStateException) { Debug.LogError("Error with DashThread Thread"); }
    }
    #endregion

    #region Thread Conditions
    private void Stun(float _time) {
        // add stunIndex
        stunIndex++;
        int currIndex = stunIndex;

        // set player stunned
        stunned = true;
        CustomDebug.Log("<b>" + name + "</b>s <color=magenta>Stun</color> start", "Codition");

        // set movemntspeed to 0
        instance.velocity.x = 0;

        // wait till player isn't stunned
        Thread.Sleep(Convert.ToInt32(Util.ConvertSecondsToMilliseconds(Convert.ToDouble(_time))));

        // check if stun should end or if there is a newer stun
        if (currIndex == stunIndex) { stunned = false; CustomDebug.Log("<b>" + name + "</b>s <color=magenta>Stuny/color> stop", "Condition"); }
    }

    private void SlowOverTime(int _time) {
        // add  slowIndex
        slowIndex++;
        int currIndex = slowIndex;
        
        // set player slowed
        slowedOverTime = true;
        CustomDebug.Log("<b>" + name + "</b>s <color=magenta>SlowOverTime</color> start", "Condition");

        // set speed
        currSpeed = slowedSpeed;

        // wait till player isn't slowed
        Thread.Sleep(_time);

        // check if slow should end or if there is a newer slow
        if (currIndex == slowIndex) {
            
            // set player to not slowed
            slowedOverTime = false;
            CustomDebug.Log("<b>" + name + "</b>s <color=magenta>SlowOverTime</color> stop", "Condition");

            // set speed to noemal speed
            if (!Slowed) { currSpeed = moveSpeed; }
            if (launching) { currSpeed = launchSpeed; }
        }
    }

    private void KnockUp(float _height)
    {
        // per cent of knockup
        float knockUpPerCent = 1f;
        knockUpPerCent *= 4;
        
        // add knockUpIndex
        knockUpIndex++;
        int currIndex = knockUpIndex;

        // get in air time
        float inAirTime = Mathf.Sqrt(Mathf.Abs(_height / 2 / -instance.gravity));

        // get in strength
        float knockUpStrenght = 2 * -instance.gravity + inAirTime;

        // set player knockUped
        knockUped = true;
        CustomDebug.Log("<b>" + name + "</b> is <color=magenta>KnockUped</color> for <color=magenta>" + inAirTime * knockUpPerCent + "</color> sec", "Condition");

        // add velocity
        instance.velocity.x = 0;
        if (_height < 0) { instance.velocity.y = -knockUpStrenght * inAirTime; }
        else { instance.velocity.y = knockUpStrenght * inAirTime; }

        // save time
        knockUpTime = inAirTime;

        // wait till player isn't knockUped
        Thread.Sleep(Convert.ToInt32(Util.ConvertSecondsToMilliseconds(Convert.ToDouble(inAirTime * knockUpPerCent))));

        // check if knockUp should end or if there is a newer nockUp
        if (currIndex == knockUpIndex) { knockUped = false; CustomDebug.Log("<b>" + name + "</b>s <color=magenta>KnockUp</color> stop", "Condition"); }

    }
    
    private void KnockBack(float _xDistance, float _time)
    {
        // add knockBackIndex
        knockBackIndex++;
        int currIndex = knockBackIndex;

        // set player knockedBack
        knockBacked = true;
        CustomDebug.Log("<b>" + name + "</b> is <color=magenta>KnockBacked</color> for <color=magenta>" + _time + "</color> sec", "Condition");

        // add velocity
        instance.velocity.x = _xDistance / _time;

        // wait till player isn't knockBacked
        Thread.Sleep(Convert.ToInt32(Util.ConvertSecondsToMilliseconds(Convert.ToDouble(_time))));

        // check if knockBack should end or if there is a newer knockBack
        if (currIndex == knockBackIndex) { knockBacked = false; CustomDebug.Log("<b>" + name + "</b>s <color=magenta>KockBack</color> stop", "Condition"); }
    }

    private void Dash(float _xDistance, float _time)
    {
        // set player to dashing
        dashing = true;
        CustomDebug.Log("<b>" + name + "</b>s <color=manta>Dash</color> start", "Condition");

        // add velocity
        instance.velocity.x = _xDistance / _time;

        // wait till dash is finished
        Thread.Sleep(Convert.ToInt32(Util.ConvertSecondsToMilliseconds(Convert.ToDouble(_time))));
        
        // stop dashing
        dashing = false;
        virgoDash = false;
        CustomDebug.Log("<b>" + name + "</b>s <color=magenta>Dash</color> stop", "Condition");
    }
    #endregion
    #endregion
    #endregion
}

