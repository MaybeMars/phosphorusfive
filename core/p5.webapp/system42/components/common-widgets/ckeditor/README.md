HTML WYSIWYG editor for Phosphorus Five
==========

Notice, this module is built on top of CKEditor, which is copyright (c) 2003-2016, CKSource - Frederico Knabben. 

CKEditor is licensed under the following licenses (which you can choose from)

* GPL
* LGPL
* MPL (Mozilla Public License)

Read more [here](http://ckeditor.com)

This "app" creates a single Active Event for you, called *[sys42.widgets.ck-editor]*, which wraps CKEditor as a 
"custom widget" on your page. To use it, you could use something like the code below.

```
create-widget
  parent:content
  class:col-xs-12
  style:"display:inline-block;width:100%;"
  widgets
    sys42.widgets.ck-editor:my-editor
```

Notice, the *[style]* attribute above, is unfortunately necessary to make sure CKEditor doesn't render in a "funny way". Feel freeto play around
with it, or add it into a CSS class in a stylesheet file, if you wish.

To get to HTML created by it, simply use *[get-widget-property]*, and pass in *[value]* as the argument of what to retrieve.
Example code given below.

```
create-widget
  parent:content
  class:col-xs-12
  style:"display:inline-block;width:100%;"
  widgets
    sys42.widgets.ck-editor:my-editor
    button
      innerValue:Get HTML
      class:btn btn-default btn-attach-top
      onclick
        get-widget-property:my-editor
          value
        sys42.windows.show-lambda:x:/..
```

To find out which arguments you can pass into it, you can use the generic lambda object, retrieving the *[_defaults]* section
of an Active Event, such as illustrated below.

```
sys42.widgets.ck-editor
  insert-before:x:
    src:x:/../*/_defaults/*
  return
```

In addition to the "arguments" you can pass into it, you can also pass in the following.

* [innerValue] - To set the initial Hyperlambda as the widget is loaded
* [events] - To associate lambda events with the widget

All other properties are ignored, and to the most parts, don't really give any sense, since the HTML "textarea" widget rendered, is actually
completely replaced at the client-side of things, by the CKEditor's internals.

