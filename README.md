# Morpeh.Events [![Github license](https://img.shields.io/github/license/codewriter-packages/Morpeh.Events.svg?style=flat-square)](#) [![Unity 2020.1](https://img.shields.io/badge/Unity-2020.3+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/Morpeh.Events?style=flat-square)
_Events for Morpeh ECS_

## How to use?

```csharp
using System;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Systems;

[Serializable]
public struct DamageRequest : IEventData {
    public EntityId targetEntityId;
}

[Serializable]
public struct DamagedEvent : IEventData {
    public EntityId targetEntityId;
}

public class DamageSystem : UpdateSystem {
    private Event<DamageRequest> damageRequest;
    private Event<DamagedEvent> damagedEvent;

    public override void OnAwake() {
        damageRequest = World.GetEvent<DamageRequest>();
        damagedEvent = World.GetEvent<DamagedEvent>();
    }

    public override void OnUpdate(float deltaTime) {
        if (!damageRequest.IsPublished) {
            return;
        }

        foreach (var evt in damageRequest.BatchedChanges) {
            ApplyDamage(evt.targetEntityId);

            damagedEvent.NextFrame(new DamagedEvent {
                targetEntityId = evt.targetEntityId,
            });
        }
    }

    private void ApplyDamage(EntityId target) { }
}
```

## License

Morpeh.Events is [MIT licensed](./LICENSE.md).
