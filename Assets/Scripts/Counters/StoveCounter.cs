using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class StoveCounter : BaseCounter,IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public class OnStateChangedEventArgs : EventArgs{
        public State state;
    }

    public enum State{
        Idle,
        Frying,
        Fried,
        Burned,
    }
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State state;
    private float fryingTimer;
    private FryingRecipeSO fryingRecipeSO;
    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    

    private void Start()
    {
        state = State.Idle;
    }

    private void Update(){
        if (!HasKitchenObject()) return;

        switch(state){
            case State.Idle:
                break;
            case State.Frying:
                fryingTimer += Time.deltaTime;

                OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
                    progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                });

                if(fryingTimer > fryingRecipeSO.fryingTimerMax){
                    // Fried

                    GetKitchenObject().DestroySelf();

                    KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                    Debug.Log("Object Fried !");

                    state = State.Fried;
                    burningTimer = 0f;
                    burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                        state = state
                    });                    

                }
                
                Debug.Log(fryingTimer);
                break;
            case State.Fried:            
                burningTimer += Time.deltaTime;

                OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
                    progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                });

                if(burningTimer > burningRecipeSO.burningTimerMax){
                    // Burned

                    GetKitchenObject().DestroySelf();

                    KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                    // Debug.Log("Object Burned !");
                    state = State.Burned;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                        state = state
                    });

                    OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
                        progressNormalized = 0f
                    });
                }
                break;
            case State.Burned:
                break;
        }
        Debug.Log(state);
    }

    public override void Interact(Player player)
    {
        if(!HasKitchenObject()){
            // There is no KitchenObject here
            if(player.HasKitchenObject()){
                // Player is carrying something
                if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())){
                    // Player carrying something that can be Fried
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    state = State.Frying;
                    fryingTimer = 0f;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                        state = state
                    });

                    OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });
                }
            }else{
                // Player not carrying anything
            }
        }else{
            // There is KitchenObject here
            if(player.HasKitchenObject()){
                // Player is carrying something                
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    // Player is Holding a Plate
                    if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())){
                        GetKitchenObject().DestroySelf();

                        
                        state = State.Idle;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                            state = state
                        });

                        OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
                            progressNormalized = 0f
                        });
                    }
                }
            } else {
                // Player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
                state = State.Idle;

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                    state = state
                });

                OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
                    progressNormalized = 0f
                });
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO){
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if(fryingRecipeSO == null){
            // Debug.LogError("input에 해당하는 fried output이 감지되지 않았음!");
            Debug.Log("input에 해당하는 fried output이 감지되지 않았음!");
            return null;
        }
        return fryingRecipeSO.output;
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach(FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray){
            if(fryingRecipeSO.input == inputKitchenObjectSO){
                return fryingRecipeSO;
            }
        }
        // Debug.LogError("input에 해당하는 fried output이 감지되지 않았음!");
        Debug.Log("input에 해당하는 fried output이 감지되지 않았음!");
        return null;
    }

    // 이하 3개는 Burning에 관한 메서드
    // private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
    //     FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
    //     return fryingRecipeSO != null;
    // }

    // private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO){
    //     FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
    //     if(fryingRecipeSO == null){
    //         // Debug.LogError("input에 해당하는 fried output이 감지되지 않았음!");
    //         Debug.Log("input에 해당하는 fried output이 감지되지 않았음!");
    //         return null;
    //     }
    //     return fryingRecipeSO.output;
    // }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach(BurningRecipeSO burningRecipeSO in burningRecipeSOArray){
            if(burningRecipeSO.input == inputKitchenObjectSO){
                return burningRecipeSO;
            }
        }
        // Debug.LogError("input에 해당하는 fried output이 감지되지 않았음!");
        Debug.Log("input에 해당하는 Burning output이 감지되지 않았음!");
        return null;
    }
}
