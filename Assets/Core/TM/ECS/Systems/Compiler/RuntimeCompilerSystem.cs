using System;
using System.Collections.Generic;
using Entitas;
using NLog;
using TM.Data;
using TM.WWW;

namespace TM.ECS.Systems.Compiler
{
    public sealed class RuntimeCompilerSystem : ReactiveSystem<GameEntity>
    {
        public RuntimeCompilerSystem(Contexts context) : base(context.game)
        {

        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) => context.CreateCollector(GameMatcher.CsCode);

        protected override bool Filter(GameEntity entity) => entity.hasCsCode & entity.hasType;

        protected override void Execute(List<GameEntity> entities)
        {

            foreach (var entity in entities)
            {
                var request = new RequestCompiler(entity.csCode.Value);
                LogManager.GetCurrentClassLogger().Info($"Compile started... Entity: {entity}");
                request.OnFinish += response =>
                {
                    ResponseCompiler responseCompiler = (ResponseCompiler) response;
                    Type compiledType = responseCompiler.CompiledType;
                    entity.ReplaceType(compiledType);
                    LogManager.GetCurrentClassLogger()
                        .Info(
                            $"<Color=Green><b>Compile code successful! Time = {responseCompiler.Milliseconds} ms.</b></Color>");
                };
                request.OnError += s =>
                {
                    //entity.ReplaceType(null);
                };
            }
        }
    }
}
