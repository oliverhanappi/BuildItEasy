using System;
using System.Collections.Generic;

namespace BuildItEasy
{
    public interface ICollectionCustomizer<TItemBuilder>
        where TItemBuilder : IBuilder
    {
        ICollectionCustomizer<TItemBuilder> AtLeast(int minCount);
        ICollectionCustomizer<TItemBuilder> Exactly(int count);
        ICollectionCustomizer<TItemBuilder> AtMost(int maxCount);

        ICollectionCustomizer<TItemBuilder> WithAll(Customizer<TItemBuilder> customizer);

        ICollectionCustomizer<TItemBuilder> With(int index, Customizer<TItemBuilder> customizer);

        ICollectionCustomizer<TItemBuilder> WithExactly(params Customizer<TItemBuilder>[] customizers);
    }

    public class ChildrenBuilder<TParent, TParentBuilder, TChild, TChildBuilder>
        where TParentBuilder : Builder<TParent>
        where TChildBuilder : Builder<TChild>
    {
        public void Customize(CollectionCustomizer<TChildBuilder> customizer)
        {
            
        }
        
        public IReadOnlyList<TChild> BuildChildren(TParent parent, TParentBuilder parentBuilder)
        {
            return null;
        }
    }
}
