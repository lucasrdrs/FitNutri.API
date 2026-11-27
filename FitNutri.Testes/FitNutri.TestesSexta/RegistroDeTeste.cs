public static class RegistroDeTeste
{
    public static void Etapa(string mensagem)
    {
        Escrever("ETAPA", mensagem);
    }

    public static void Sucesso(string mensagem)
    {
        Escrever("SUCESSO", mensagem);
    }

    public static void Aviso(string mensagem)
    {
        Escrever("AVISO", mensagem);
    }

    public static void Erro(string mensagem)
    {
        Escrever("ERRO", mensagem);
    }

    private static void Escrever(string tipo, string mensagem)
    {
        string registro =
            $"[{DateTime.Now:HH:mm:ss}] [{tipo}] {mensagem}";

        Console.WriteLine(registro);
        TestContext.Progress.WriteLine(registro);
    }


}
