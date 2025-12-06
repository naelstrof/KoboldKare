public class ReactionsDatabase : Database<ScriptableReagentReaction> {
    public static void DoReactions(GenericReagentContainer container) {
        foreach(var pair in instance.assets) {
            pair.Value.DoReaction(container);
        }
    }
}
