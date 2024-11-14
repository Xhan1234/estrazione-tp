using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;


namespace MyWebApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // Index method to handle task listing with date range filters
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
            {
                // Return an empty list or message if no filters are applied
                return View(new List<TaskData>());
            }

            try
            {
                var adjustedEndDate = endDate.Value.AddDays(1).ToString("yyyy-MM-dd");

                var tasksQuery = _context.TaskDatas
                    .FromSqlRaw(@"
                                SELECT 
                                    n.Code AS Rete,
                                    nd.Name AS NomeRete,
                                    p.Code AS Pdv,
                                    lt.Id AS IdTask,
                                    lt.CreatedOn AS DataCreazioneTask,
                                    (
                                        SELECT TOP 1 TraceUpdateTime
                                        FROM ShipmentArticleOperations sao
                                        WHERE sao.LogisticTaskId = lt.Id
                                        AND sao.PhaseCode = 'CONS'
                                    ) AS DataConsegna,
                                    (
                                        SELECT TOP 1 sao.StatusDescription
                                        FROM ShipmentArticleOperations sao
                                        WHERE sao.LogisticTaskId = lt.Id
                                        ORDER BY TraceUpdateTime DESC
                                    ) AS StatoBRT,
                                    pst.DeliveryWaybill AS LetteraDiVettura,
                                    (
                                        SELECT COUNT(*)
                                        FROM TaskArticles ta WITH(NOLOCK)
                                        WHERE ta.LogisticTaskId = lt.Id
                                        AND ta.Type IN (1,2,6)
                                        AND ta.Status = 0
                                    ) AS QuantitaOBU,
                                    (
                                        SELECT TOP 1 
                                            CASE 
                                                WHEN Type = 1 THEN 'Italiano'
                                                WHEN Type = 2 THEN 'Europeo'
                                                ELSE 'Colorato'
                                            END
                                        FROM TaskArticles ta WITH(NOLOCK)
                                        WHERE ta.LogisticTaskId = lt.Id
                                        AND ta.Type IN (1,2,6)
                                        AND ta.Status = 0
                                    ) AS TipologiaOBU,
                                    CASE 
                                        WHEN pst.TaskStatus = -3 THEN 'Non valido'
                                        WHEN pst.TaskStatus = -2 THEN 'Da verificare'
                                        WHEN pst.TaskStatus = -1 THEN 'Nuovo'
                                        WHEN pst.TaskStatus =  0 THEN 'In attesa file'
                                        WHEN pst.TaskStatus =  1 THEN 'In attesa materiale'
                                        WHEN pst.TaskStatus =  2 THEN 'Quadratura materiale'
                                        WHEN pst.TaskStatus =  3 THEN 'Chiuso'
                                        WHEN pst.TaskStatus =  4 THEN 'In transito'
                                        WHEN pst.TaskStatus =  5 THEN 'In Lavorazione'
                                        WHEN pst.TaskStatus =  6 THEN 'Pronto per la spedizione da Hub'
                                        WHEN pst.TaskStatus =  7 THEN 'In attesa corriere'
                                        WHEN pst.TaskStatus =  8 THEN 'Mancato ritiro'
                                        WHEN pst.TaskStatus =  9 THEN 'In attesa cassone'
                                        ELSE ''
                                    END AS StatoTask,
                                    'Spedizione diretta' AS TipologiaTask,
                                    CASE
                                        WHEN pst.IsFirstSupply = 1 THEN 'Si'
                                        ELSE 'No'
                                    END AS PrimoApprovvigionamento
                                FROM PdvSupplyTasks pst WITH(NOLOCK)
                                INNER JOIN LogisticTasks lt WITH(NOLOCK) ON pst.LogisticTaskId = lt.Id
                                INNER JOIN Pdvs p WITH(NOLOCK) ON lt.LogisticEntityId = p.Id
                                INNER JOIN PdvDetails pd WITH(NOLOCK) ON pd.PdvId = p.Id AND pd.IsLast = 1
                                INNER JOIN Networks n WITH(NOLOCK) ON pd.NetworkId = n.Id
                                INNER JOIN NetworkDetails nd WITH(NOLOCK) ON nd.NetworkId = n.Id AND nd.IsLast = 1
                                WHERE pst.IsLast = 1
                                AND lt.CreatedOn >= {0} AND lt.CreatedOn < {1}
                                AND pd.Status = 0
                                GROUP BY 
                                    n.Code, nd.Name, p.Code, lt.Id, lt.CreatedOn, pst.DeliveryWaybill, 
                                    pst.TaskStatus, pst.IsFirstSupply
                                ORDER BY p.Code, lt.CreatedOn;", startDate, adjustedEndDate);

                var tasks = await tasksQuery.ToListAsync();

                Console.WriteLine($"Retrieved {tasks.Count} tasks.");  // Log the number of tasks retrieved
                return View(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }
    }
}