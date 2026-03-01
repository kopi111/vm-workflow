using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Application.Interfaces;

public interface IScriptGenerationService
{
    string GenerateFortiGateScript(Request request);
}
