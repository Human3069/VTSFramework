namespace VTSFramework.TSModule
{
    public interface IRow<T> where T : IRow<T>
    {
        // ���̺��� ������ �����Ͽ� �������ִ� �Լ�
        // ��) ������� �� \n, ��ҹ��ڸ� ����ȭ����.
        T Validated();
    }
}