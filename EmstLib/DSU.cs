namespace EmstLib;

public class DSU
{
    private readonly int[] parent;
    private readonly int[] size;
    
    public DSU(int n)
    {
        parent = new int[n];
        size = new int[n];
        for (int i = 0; i < n; i++)
        {
            parent[i] = i;
            size[i] = 1;
        }
    }
    
    public int Find(int x)
    {
        if (parent[x] != x)
            parent[x] = Find(parent[x]);
        return parent[x];
    }
    
    public bool Union(int a, int b)
    {
        a = Find(a);
        b = Find(b);
        if (a == b) return false;
        
        if(size[a] < size[b])
        {
            int temp = a;
            a = b;
            b = temp;
        }
        parent[b] = a;
        size[a] += size[b];

        return true;
    }
}