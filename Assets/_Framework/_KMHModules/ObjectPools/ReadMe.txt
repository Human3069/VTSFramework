1. BaseObjectPoolManager : ObjectPoolManager�� �������� ObjectPoolHandler �߻�Ŭ������ ���ԵǾ��ֽ��ϴ�.
	- ObjectPoolManager : ��ųʸ��� �������� ObjectPoolHandler�� �����մϴ�.
	- ObjectPoolHandler : �ϳ��� ������Ʈ�� Queue�� �����մϴ�.

2. BaseObjectPoolController : �ϳ��� ObjectPoolController �߻�Ŭ������ ���ԵǾ��ֽ��ϴ�.
	- ObjectPoolController : �ϳ��� ������Ʈ�� Queue�� �����մϴ�.

BaseObjectPoolManager �� BaseObjectPoolController�� �������� Queue�� ������ �ֽ��ϴ�.
BaseObjectPoolManager �� �������� Queue�� ���������� BaseObjectPoolController �� �׷��� �ʽ��ϴ�.
���� BaseObjectPoolController�� SoundManager �� UIManager�� ���� �ٸ� Ŭ������ �����ϵ��� ����Ǿ����ϴ�.