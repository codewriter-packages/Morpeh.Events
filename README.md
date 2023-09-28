# Morpeh.Events [![Github license](https://img.shields.io/github/license/codewriter-packages/Morpeh.Events.svg?style=flat-square)](#) [![Unity 2020.1](https://img.shields.io/badge/Unity-2020.3+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/Morpeh.Events?style=flat-square)
_Lightweight events and requests for [Morpeh](https://github.com/scellecs/morpeh)_

## :receipt: Core Concepts

:bookmark: **Request** is a queued command that can consumed in the same frame in which it was sent. A request can be sent from multiple systems, but only one system must handle it.<br/>

> [!IMPORTANT]
> If you publish requests but do not consume them this may lead to a memory leak because all requests are stored until they are consumed.

```csharp
// get request
var damageRequest = World.GetRequest<DamageRequest>();

// send request
damageRequest.Publish(new DamageRequest { ... });

// handle request
foreach (var request in damageRequest.Consume()) { ... }
```

<hr/>

:bookmark: **Event** is a one-frame action that is processed on the next frame after it is sent. An event can be processed in many systems. Good practice is to send events only from one system.<br/>

> [!IMPORTANT]  
> Events live only for one frame so it is necessary to process events every frame otherwise some events may be lost.

```csharp
// get event
var damageEvent = World.GetEvent<DamageEvent>();

// send event
damageEvent.NextFrame(new DamageEvent { ... });

// handle event
foreach (var evt in damagedEvent.publishedChanges) { ... }

// subscribe to event
var subscription = damagedEvent.Subscribe(changes => { ... });

// do not forget to unsubscribe from event 
subscription.Dispose();

```

## :thought_balloon: How to use?

Assume that the game has a damage mechanics. So we create `DamageRequest` and `DamageEvent`. Damage request can be sent from `AttackSystem`, `ExplosionSystem` and so on. 
Then `DamageSystem` handles that request and send damage event. Damage event can be handled in `PlaySoundOnDamageSystem`, `ExplodeOnDeathSystem` and others.

<p align="center">
    <img src="https://github.com/codewriter-packages/Morpeh.Events/assets/26966368/51b7de6c-9bd7-430b-b417-9c31da1bcfe4" width="850" height="160">
</p>

<hr/>

<details>
  <summary>Click to show demo code</summary>

```csharp
using System;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Systems;
```

```csharp
public struct DamageRequest : IRequestData {
    public EntityId targetEntityId;
}
```

```csharp
public struct DamagedEvent : IEventData {
    public EntityId targetEntityId;
}
```

```csharp
public class DamageSystem : UpdateSystem {
    private Request<DamageRequest> damageRequest;
    private Event<DamagedEvent> damagedEvent;

    public override void OnAwake() {
        damageRequest = World.GetRequest<DamageRequest>();
        damagedEvent = World.GetEvent<DamagedEvent>();
    }

    public override void OnUpdate(float deltaTime) {
        foreach (var request in damageRequest.Consume()) {
            ApplyDamage(request.targetEntityId);

            damagedEvent.NextFrame(new DamagedEvent {
                targetEntityId = request.targetEntityId,
            });
        }
    }

    private void ApplyDamage(EntityId target) { }
}
```

```csharp
public class PlaySoundOnDamageSystem : UpdateSystem {
    private Event<DamagedEvent> damagedEvent;

    public override void OnAwake() {
        damagedEvent = World.GetEvent<DamagedEvent>();
    }

    public override void OnUpdate(float deltaTime) {
        foreach (var evt in damagedEvent.publishedChanges) {
            PlaySound(evt.targetEntityId);
        }
    }

    private void PlaySound(EntityId target) { }
}
```

</details>

<hr/>

## :open_book: How to Install

Library distributed as git package ([How to install package from git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html))
<br>Git URL: `https://github.com/codewriter-packages/Morpeh.Events.git`

## :green_book: License

Morpeh.Events is [MIT licensed](./LICENSE.md).
