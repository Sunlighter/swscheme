/*
    This file is part of Sunlit World Scheme
    http://swscheme.codeplex.com/
    Copyright (c) 2010 by Edward Kiser (edkiser@gmail.com)

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Symbol = ExprObjModel.Symbol;
using DescendantsWithPatterns = ExprObjModel.DescendantsWithPatternsAttribute;
using Pattern = ExprObjModel.PatternAttribute;
using Bind = ExprObjModel.BindAttribute;

namespace Pascalesque.Two.Syntax
{
    [Pattern("$elements")]
    public class ModuleSyntax
    {
        [Bind("$elements")]
        public List<ElementOfModuleSyntax> elements;

        public ModuleToBuild GetModuleToBuild()
        {
            return new ModuleToBuild(elements.Select(x => x.GetElement()));
        }
    }
    
    [DescendantsWithPatterns]
    public abstract class ElementOfModuleSyntax
    {
        public abstract ElementOfModule GetElement();
    }

    [Pattern("(class $name . $elems)")]
    public class ClassToBuildSyntax : ElementOfModuleSyntax
    {
        [Bind("$name")]
        public Symbol name;

        [Bind("$elems")]
        public List<ElementOfClassSyntax> elements;

        private TypeReference GetAncestor()
        {
            TypeReference ancestor = null;
            List<ExtendsSyntax> extends = elements.OfType<ExtendsSyntax>().ToList();
            if (extends.Count == 0)
            {
                ancestor = ExistingTypeReference.Object;
            }
            else if (extends.Count == 1)
            {
                ancestor = extends[0].ancestor.GetTheType();
            }
            else
            {
                throw new PascalesqueException("Class cannot have multiple ancestors");
            }
            return ancestor;
        }

        private TypeAttributes GetAttributes()
        {
            TypeAttributes t = (TypeAttributes)0;
            foreach (TypeAttributeSyntax a in elements.OfType<TypeAttributeSyntax>())
            {
                t = a.SetAttribute(t);
            }
            return t;
        }

        private IEnumerable<TypeReference> GetInterfaces()
        {
            List<ImplementsSyntax> implements = elements.OfType<ImplementsSyntax>().ToList();
            return implements.SelectMany(x => x.interfaces.Select(y => y.GetTheType()));
        }

        public override ElementOfModule GetElement()
        {
            return new ClassToBuild
            (
                name,
                GetAttributes(),
                GetAncestor(),
                GetInterfaces(),
                elements.Where(x => x.HasTrueElement).Select(x => x.GetElement())
            );
        }
    }

    [DescendantsWithPatterns]
    public abstract class ElementOfClassSyntax
    {
        public abstract bool HasTrueElement { get; }

        public abstract ElementOfClass GetElement();
    }

    [DescendantsWithPatterns]
    public abstract class TypeAttributeSyntax : ElementOfClassSyntax
    {
        public override bool HasTrueElement { get { return false; } }

        public override ElementOfClass GetElement()
        {
 	        throw new NotImplementedException();
        }

        public abstract TypeAttributes SetAttribute(TypeAttributes t);
    }

    [Pattern("public")]
    public class PublicTypeAttributeSyntax : TypeAttributeSyntax
    {
        public override TypeAttributes SetAttribute(TypeAttributes t)
        {
            return t | TypeAttributes.Public;
        }
    }

    [Pattern("abstract")]
    public class AbstractTypeAttributeSyntax : TypeAttributeSyntax
    {
        public override TypeAttributes SetAttribute(TypeAttributes t)
        {
            return t | TypeAttributes.Abstract;
        }
    }

    [Pattern("sealed")]
    public class SealedTypeAttributeSyntax : TypeAttributeSyntax
    {
        public override TypeAttributes SetAttribute(TypeAttributes t)
        {
            return t | TypeAttributes.Sealed;
        }
    }

    [Pattern("static")]
    public class StaticTypeAttributeSyntax : TypeAttributeSyntax
    {
        public override TypeAttributes SetAttribute(TypeAttributes t)
        {
            return t | TypeAttributes.Abstract | TypeAttributes.Sealed;
        }
    }

    [Pattern("serializable")]
    public class SerializableTypeAttributeSyntax : TypeAttributeSyntax
    {
        public override TypeAttributes SetAttribute(TypeAttributes t)
        {
            return t | TypeAttributes.Serializable;
        }
    }

    [Pattern("(extends $class)")]
    public class ExtendsSyntax : ElementOfClassSyntax
    {
        [Bind("$class")]
        public TypeSyntax ancestor;

        public override bool HasTrueElement { get { return false; } }

        public override ElementOfClass GetElement()
        {
            throw new NotImplementedException();
        }
    }

    [Pattern("(implements . $interfaces)")]
    public class ImplementsSyntax : ElementOfClassSyntax
    {
        [Bind("$interfaces")]
        public List<TypeSyntax> interfaces;

        public override bool HasTrueElement { get { return false; } }

        public override ElementOfClass GetElement()
        {
 	        throw new NotImplementedException();
        }
    }

    [Pattern("(method $returntype $name $attribs $body)")]
    public class MethodSyntax : ElementOfClassSyntax
    {
        [Bind("$name")]
        public Symbol name;

        [Bind("$returntype")]
        public TypeSyntax returnType;

        [Bind("$attribs")]
        public List<MethodAttributeSyntax> attribs;

        [Bind("$body")]
        public LambdaSyntax body;

        private MethodAttributes GetAttributes()
        {
            MethodAttributes m = MethodAttributes.HideBySig;
            foreach (MethodAttributeSyntax a in attribs)
            {
                m = a.SetAttribute(m);
            }
            return m;
        }

        public override bool HasTrueElement
        {
            get { return true; }
        }

        public override ElementOfClass GetElement()
        {
            return new MethodToBuild
            (
                name,
                GetAttributes(),
                returnType.GetTheType(),
                (LambdaExpr2)(body.GetExpr())
            );
        }
    }

    [DescendantsWithPatterns]
    public abstract class MethodAttributeSyntax
    {
        public abstract MethodAttributes SetAttribute(MethodAttributes m);
    }

    [Pattern("public")]
    public class PublicMethodAttributeSyntax : MethodAttributeSyntax
    {
        public override MethodAttributes SetAttribute(MethodAttributes m)
        {
            return m | MethodAttributes.Public;
        }
    }

    [Pattern("virtual")]
    public class VirtualMethodAttributeSyntax : MethodAttributeSyntax
    {
        public override MethodAttributes SetAttribute(MethodAttributes m)
        {
            return m | MethodAttributes.Virtual;
        }
    }

    [Pattern("new")]
    public class NewMethodAttributeSyntax : MethodAttributeSyntax
    {
        public override MethodAttributes SetAttribute(MethodAttributes m)
        {
            return m | MethodAttributes.NewSlot;
        }
    }

    [Pattern("private")]
    public class PrivateMethodAttributeSyntax : MethodAttributeSyntax
    {
        public override MethodAttributes SetAttribute(MethodAttributes m)
        {
            return m | MethodAttributes.Private;
        }
    }

    [Pattern("static")]
    public class StaticMethodAttributeSyntax : MethodAttributeSyntax
    {
        public override MethodAttributes SetAttribute(MethodAttributes m)
        {
            return m | MethodAttributes.Static;
        }
    }

    [Pattern("final")]
    public class FinalMethodAttributeSyntax : MethodAttributeSyntax
    {
        public override MethodAttributes SetAttribute(MethodAttributes m)
        {
            return m | MethodAttributes.Final;
        }
    }

    [Pattern("special")]
    public class SpecialMethodAttributeSyntax : MethodAttributeSyntax
    {
        public override MethodAttributes SetAttribute(MethodAttributes m)
        {
            return m | MethodAttributes.SpecialName;
        }
    }

    [Pattern("(field $type $name $attribs)")]
    public class FieldSyntax : ElementOfClassSyntax
    {
        [Bind("$type")]
        public TypeSyntax fieldType;

        [Bind("$name")]
        public Symbol name;

        [Bind("$attribs")]
        public List<FieldAttributeSyntax> attribs;

        private FieldAttributes GetAttributes()
        {
            FieldAttributes f = (FieldAttributes)0;
            return f;
        }

        public override bool HasTrueElement { get { return true; } }

        public override ElementOfClass GetElement()
        {
            return new FieldToBuild
            (
                GetAttributes(),
                fieldType.GetTheType(),
                name
            );
        }
    }

    [DescendantsWithPatterns]
    public abstract class FieldAttributeSyntax
    {
        public abstract FieldAttributes SetAttribute(FieldAttributes f);
    }

    [Pattern("public")]
    public class PublicFieldAttributeSyntax : FieldAttributeSyntax
    {
        public override FieldAttributes SetAttribute(FieldAttributes f)
        {
            return f | FieldAttributes.Public;
        }
    }

    [Pattern("static")]
    public class StaticFieldAttributeSyntax : FieldAttributeSyntax
    {
        public override FieldAttributes SetAttribute(FieldAttributes f)
        {
            return f | FieldAttributes.Static;
        }
    }

    [Pattern("transient")]
    public class TransientFieldAttributeSyntax : FieldAttributeSyntax
    {
        public override FieldAttributes SetAttribute(FieldAttributes f)
        {
            return f | FieldAttributes.NotSerialized;
        }
    }

    [Pattern("(constructor $attribs $body)")]
    public class ConstructorSyntax : ElementOfClassSyntax
    {
        [Bind("$attribs")]
        public List<MethodAttributeSyntax> attribs;

        [Bind("$body")]
        public LambdaSyntax body;

        private MethodAttributes GetAttributes()
        {
            MethodAttributes m = (MethodAttributes)0;
            foreach (MethodAttributeSyntax a in attribs)
            {
                m = a.SetAttribute(m);
            }
            return m;
        }

        public override bool HasTrueElement { get { return true; } }

        public override ElementOfClass GetElement()
        {
            return new ConstructorToBuild
            (
                GetAttributes(),
                (LambdaExpr2)(body.GetExpr())
            );
        }
    }
}