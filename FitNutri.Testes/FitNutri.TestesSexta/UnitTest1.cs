using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

[TestFixture]
public class TesteCadastro
{
    private const string UrlCadastro =
        "http://127.0.0.1:5500/telas/cadastro.html";

    string NomeUsuario = "teste" + System.DateTime.Now.ToString("HHmmss");

    private static readonly string Email =
        $"teste{DateTime.Now:yyyyMMddHHmmss}@example.com";

    private const string Senha = "Teste123!";

    [Test]
    public void DeveCadastrarContaComDadosValidos()
    {
        RegistroDeTeste.Etapa("Iniciando teste de cadastro válido.");

        try
        {
            using IWebDriver navegador = AbrirPaginaCadastro();

            RegistroDeTeste.Etapa("Preenchendo os dados válidos.");

            EscreverDevagar(
                navegador.FindElement(By.Id("registerUserName")),
                NomeUsuario,
                50
            );

            EscreverDevagar(
                navegador.FindElement(By.Id("registerEmail")),
                Email,
                50
            );

            EscreverDevagar(
                navegador.FindElement(By.Id("registerPassword")),
                Senha,
                50
            );

            EscreverDevagar(
                navegador.FindElement(By.Id("confirmPassword")),
                Senha,
                50
            );

            RegistroDeTeste.Etapa("Clicando em Criar conta.");

            navegador.FindElement(
                By.CssSelector("#registerForm button[type='submit']")
            ).Click();

            // Aguarda até 15 segundos para a API aceitar o cadastro
            // e a página avançar para onboarding.html.
            DateTime limite = DateTime.Now.AddSeconds(15);

            while (!navegador.Url.Contains(
                       "onboarding.html",
                       StringComparison.OrdinalIgnoreCase)
                   && DateTime.Now < limite)
            {
                Thread.Sleep(250);
            }

            if (!navegador.Url.Contains(
                    "onboarding.html",
                    StringComparison.OrdinalIgnoreCase))
            {
                // Se não avançou, continua na página de cadastro,
                // onde o formulário poderá mostrar uma mensagem de erro.
                string mensagem = navegador
                    .FindElement(By.Id("registerMessage"))
                    .Text
                    .Trim();

                Assert.Fail(
                    "O cadastro válido não avançou para onboarding.html. " +
                    $"Mensagem da página: {mensagem}"
                );
            }

            RegistroDeTeste.Sucesso(
                "Conta criada e página de onboarding aberta."
            );

            Thread.Sleep(3000);
        }
        catch (Exception erro)
        {
            RegistroDeTeste.Erro(
                $"Teste de cadastro válido interrompido: {erro.Message}"
            );

            throw;
        }
    }

    [Test]
    public void DeveMostrarErroQuandoAsSenhasForemDiferentes()
    {
        RegistroDeTeste.Etapa(
            "Iniciando teste de senhas diferentes."
        );

        try
        {
            using IWebDriver navegador = AbrirPaginaCadastro();

            EscreverDevagar(
                navegador.FindElement(By.Id("registerUserName")),
                NomeUsuario,
                30
            );

            EscreverDevagar(
                navegador.FindElement(By.Id("registerEmail")),
                Email,
                30
            );

            EscreverDevagar(
                navegador.FindElement(By.Id("registerPassword")),
                Senha,
                30
            );

            EscreverDevagar(
                navegador.FindElement(By.Id("confirmPassword")),
                "Outra123!",
                30
            );

            RegistroDeTeste.Etapa("Tentando enviar o cadastro.");
            navegador.FindElement(
                By.CssSelector("#registerForm button[type='submit']")
            ).Click();

            Thread.Sleep(500);

            string mensagem = navegador
                .FindElement(By.Id("registerMessage"))
                .Text;

            Assert.That(
                mensagem,
                Does.Contain("As senhas não coincidem."),
                "A página deveria avisar que as senhas são diferentes."
            );

            Assert.That(
                navegador.Url,
                Does.Contain("cadastro.html"),
                "O cadastro inválido não deveria avançar de página."
            );

            RegistroDeTeste.Sucesso(
                "A página informou que as senhas não coincidem."
            );
        }
        catch (Exception erro)
        {
            RegistroDeTeste.Erro(
                $"Teste de senhas diferentes interrompido: {erro.Message}"
            );

            throw;
        }
    }

    [Test]
    public void DeveImpedirCadastroSemEmail()
    {
        RegistroDeTeste.Etapa(
            "Iniciando teste de e-mail obrigatório vazio."
        );

        try
        {
            using IWebDriver navegador = AbrirPaginaCadastro();

            EscreverDevagar(
                navegador.FindElement(By.Id("registerUserName")),
                NomeUsuario,
                30
            );

            // O e-mail fica vazio de propósito neste cenário.
            EscreverDevagar(
                navegador.FindElement(By.Id("registerPassword")),
                Senha,
                30
            );

            EscreverDevagar(
                navegador.FindElement(By.Id("confirmPassword")),
                Senha,
                30
            );

            RegistroDeTeste.Etapa(
                "Tentando enviar sem preencher o e-mail."
            );

            navegador.FindElement(
                By.CssSelector("#registerForm button[type='submit']")
            ).Click();

            bool emailObrigatorioVazio = Convert.ToBoolean(
                ((IJavaScriptExecutor)navegador).ExecuteScript(
                    "return arguments[0].validity.valueMissing;",
                    navegador.FindElement(By.Id("registerEmail"))
                )
            );

            Assert.That(
                emailObrigatorioVazio,
                Is.True,
                "O navegador deveria impedir o envio sem e-mail."
            );

            Assert.That(
                navegador.Url,
                Does.Contain("cadastro.html"),
                "O formulário sem e-mail não deveria avançar de página."
            );

            RegistroDeTeste.Sucesso(
                "O navegador impediu o envio sem o e-mail obrigatório."
            );
        }
        catch (Exception erro)
        {
            RegistroDeTeste.Erro(
                $"Teste de e-mail vazio interrompido: {erro.Message}"
            );

            throw;
        }
    }

    [Test]
    public void DeveRejeitarSenhaAbaixoDoMinimo()
    {
        RegistroDeTeste.Etapa("Iniciando teste do limite mínimo de caracteres da senha.");

        try
        {
            using IWebDriver navegador = new ChromeDriver();


            // A senha tem 7 caracteres, abaixo do mínimo de 8.
            string senhaAbaixoDoMinimo = "Ab1!cde";

            RegistroDeTeste.Etapa("Abrindo a página de cadastro.");
            navegador.Navigate().GoToUrl(
                "http://127.0.0.1:5500/telas/cadastro.html"
            );

            IWebElement campoNomeUsuario =
                navegador.FindElement(By.Id("registerUserName"));
            IWebElement campoEmail =
                navegador.FindElement(By.Id("registerEmail"));
            IWebElement campoSenha =
                navegador.FindElement(By.Id("registerPassword"));
            IWebElement campoConfirmacao =
                navegador.FindElement(By.Id("confirmPassword"));
            IWebElement botaoCriarConta =
                navegador.FindElement(By.CssSelector("button[type='submit']"));

            RegistroDeTeste.Etapa(
                "Preenchendo o cadastro com uma senha de 7 caracteres."
            );
            campoNomeUsuario.SendKeys(NomeUsuario);
            campoEmail.SendKeys(Email);
            campoSenha.SendKeys(senhaAbaixoDoMinimo);
            campoConfirmacao.SendKeys(senhaAbaixoDoMinimo);

            RegistroDeTeste.Etapa(
                "Tentando enviar o formulário com a senha abaixo do mínimo."
            );
            botaoCriarConta.Click();

            bool senhaCurtaBloqueada = (bool)(
                (IJavaScriptExecutor)navegador
            ).ExecuteScript(
                "return arguments[0].validity.tooShort;",
                campoSenha
            );

            Assert.That(
                senhaCurtaBloqueada,
                Is.True,
                "O navegador deveria bloquear uma senha com menos de 8 caracteres."
            );

            Assert.That(
                navegador.Url,
                Does.Contain("cadastro.html"),
                "O formulário não deveria avançar com a senha abaixo do mínimo."
            );

            RegistroDeTeste.Sucesso(
                "A senha de 7 caracteres foi rejeitada; o formulário permaneceu na página de cadastro."
            );

        }
        catch (System.Exception erro)
        {
            RegistroDeTeste.Erro(
                $"Teste do limite mínimo da senha interrompido: {erro.Message}"
            );

            throw;
        }
    }

    // Abre a página de cadastro e retorna o navegador para o teste.
    private static IWebDriver AbrirPaginaCadastro()
    {
        IWebDriver navegador = new ChromeDriver();
        navegador.Navigate().GoToUrl(UrlCadastro);
        Thread.Sleep(1000);
        return navegador;
    }

    // Digita o texto caractere por caractere, com uma pausa entre as letras.
    private static void EscreverDevagar(
        IWebElement campo,
        string texto,
        int intervaloMilissegundos)
    {
        foreach (char letra in texto)
        {
            campo.SendKeys(letra.ToString());
            Thread.Sleep(intervaloMilissegundos);
        }
    }
}