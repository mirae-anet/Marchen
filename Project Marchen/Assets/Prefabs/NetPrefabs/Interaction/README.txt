소환 방식
Local Spawner -> Object

작업의 순서는 Object를 우선 만들고 Local Spawner를 만든다.

Object는 collider와 interactionHandler를 상속한 C#script을 동일한 위치에 둔다.

---

Local Spawner는 SpawnerSpawner.cs를 사용하기 위한 DetectionCollder 외에 Colldier를 가지지 않음.
이유는 스폰되는 Object랑 물리적인 충돌이 발생할 수 있음.

또한 rigidBody도 가지지 않음.

Local Spawner는 Object에서 Colldier와 RigidBody, NetworkRigidBody 등 불필요한 것들을 제외하고
DetectionCollder를 추가하면 된다.
