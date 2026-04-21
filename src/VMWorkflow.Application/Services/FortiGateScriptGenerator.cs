using System.Text;
using VMWorkflow.Application.Interfaces;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.Services;

public class FortiGateScriptGenerator : IScriptGenerationService
{
    public string GenerateFortiGateScript(Request request)
    {
        if (request.Status != RequestStatus.PendingApproval
            && request.Status != RequestStatus.Approved
            && request.Status != RequestStatus.Implemented
            && request.Status != RequestStatus.Closed)
            throw new InvalidOperationException($"Script generation requires PendingApproval status or later. Current: {request.Status}");

        if (request.NOCDetails == null)
            throw new InvalidOperationException("NOC details are required for script generation.");
        if (request.SOCDetails == null)
            throw new InvalidOperationException("SOC details are required for script generation.");

        var noc = request.NOCDetails;
        var soc = request.SOCDetails;
        var slug = request.ObjectSlug;

        var sb = new StringBuilder();
        sb.AppendLine("# FortiGate CLI Script");
        sb.AppendLine($"# Generated for: {slug}");
        sb.AppendLine($"# Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        sb.AppendLine("# --- Address Object ---");
        sb.AppendLine("config firewall address");
        sb.AppendLine($"    edit \"{slug}\"");
        sb.AppendLine($"        set subnet {noc.IPAddress}/32");
        if (!string.IsNullOrEmpty(request.FQDNSuggestion))
            sb.AppendLine($"        set fqdn \"{request.FQDNSuggestion}\"");
        sb.AppendLine($"        set comment \"Auto-generated for {request.ApplicationName}\"");
        sb.AppendLine("    next");
        sb.AppendLine("end");
        sb.AppendLine();

        foreach (var fw in soc.FirewallEntries)
        {
            sb.AppendLine($"# --- Firewall Policy: {fw.PolicyName} (VDOM: {fw.VDOM}) ---");
            sb.AppendLine("config firewall policy");
            sb.AppendLine($"    edit 0");
            sb.AppendLine($"        set name \"{fw.PolicyName}\"");
            sb.AppendLine($"        set srcintf \"{fw.SourceInterface}\"");
            sb.AppendLine($"        set dstintf \"{fw.DestinationInterface}\"");
            sb.AppendLine($"        set srcaddr \"{fw.SourceIP}\"");
            sb.AppendLine($"        set dstaddr \"{fw.DestinationIP}\"");
            var serviceNames = string.Join(" ", fw.Services.Select(s => string.IsNullOrEmpty(s.ServiceName) ? $"{s.Protocol}/{s.Port}" : s.ServiceName));
            sb.AppendLine($"        set service \"{serviceNames}\"");
            sb.AppendLine($"        set schedule \"{fw.Schedule}\"");
            sb.AppendLine($"        set action {fw.Action.ToString().ToLower()}");
            sb.AppendLine($"        set logtraffic all");
            sb.AppendLine($"        set comments \"{fw.PolicyName}\"");
            sb.AppendLine("    next");
            sb.AppendLine("end");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
