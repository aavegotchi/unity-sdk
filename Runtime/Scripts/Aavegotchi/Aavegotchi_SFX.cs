using UnityEngine;

public class Aavegotchi_SFX : MonoBehaviour
{
    [SerializeField] private AdvancedAudioSource AngrySFX;
    [SerializeField] private AdvancedAudioSource DashLoopSFX;
    [SerializeField] private AdvancedAudioSource DashEndSFX;
    [SerializeField] private AdvancedAudioSource DeathSFX;
    [SerializeField] private AdvancedAudioSource ShootSFX;
    [SerializeField] private AdvancedAudioSource HappySFX;
    [SerializeField] private AdvancedAudioSource HitSFX;
    [SerializeField] private AdvancedAudioSource MeleeSFX;
    [SerializeField] private AdvancedAudioSource SadSFX;
    [SerializeField] private AdvancedAudioSource SpookySFX;
    [SerializeField] private AdvancedAudioSource ThrowSFX;
    [SerializeField] private AdvancedAudioSource WhirlwindSFX;
    [SerializeField] private AdvancedAudioSource VictoryFinalSFX;
    [SerializeField] private AdvancedAudioSource ImmolationGainedSFX;
    [SerializeField] private AdvancedAudioSource ImmolationLostSFX;

    //--------------------------------------------------------------------------------------------------
    public void PlayAngrySFX()
    {
        if (enabled)
        {
            AngrySFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void StartDashLoop()
    {
        if (enabled)
        {
            DashLoopSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void EndDashLoop()
    {
        if (enabled)
        {
            DashEndSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlayDeathSFX()
    {
        if (enabled)
        {
            DeathSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlayShootSFX()
    {
        if (enabled)
        {
            ShootSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlayHappySFX()
    {
        if (enabled)
        {
            HappySFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlayHitSFX()
    {
        if (enabled)
        {
            HitSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlayMeleeSFX()
    {
        if (enabled)
        {
            MeleeSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlaySadSFX()
    {
        if (enabled)
        {
            SadSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlaySpookySFX()
    {
        if (enabled)
        {
            SpookySFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlayThrowSFX()
    {
        if (enabled)
        {
            ThrowSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlayWhirlwindSFX()
    {
        if (enabled)
        {
            WhirlwindSFX.Play();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void PlayVictoryFinalSFX()
    {
        if (enabled)
        {
            VictoryFinalSFX.Play();
        }
    }
    
    //--------------------------------------------------------------------------------------------------
    public void PlayImmolationGainedSFX()
    {
        if (enabled)
        {
            ImmolationGainedSFX.Play();
        }
    }
    
    //--------------------------------------------------------------------------------------------------
    public void PlayImmolationLostSFX()
    {
        if (enabled)
        {
            ImmolationLostSFX.Play();
        }
    }
}
