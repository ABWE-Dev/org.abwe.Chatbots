System.register(['vue', '@Obsidian/Controls/panel.obs', '@Obsidian/Controls/textBox.obs', '@Obsidian/Controls/numberBox.obs', '@Obsidian/Controls/rockButton.obs', '@Obsidian/Utility/block', '@Obsidian/Enums/Controls/btnType'], (function (exports) {
  'use strict';
  var createTextVNode, defineComponent, ref, openBlock, createBlock, unref, withCtx, createElementBlock, Fragment, createCommentVNode, createVNode, Panel, TextBox, NumberBox, Button, useInvokeBlockAction, BtnType;
  return {
    setters: [function (module) {
      createTextVNode = module.createTextVNode;
      defineComponent = module.defineComponent;
      ref = module.ref;
      openBlock = module.openBlock;
      createBlock = module.createBlock;
      unref = module.unref;
      withCtx = module.withCtx;
      createElementBlock = module.createElementBlock;
      Fragment = module.Fragment;
      createCommentVNode = module.createCommentVNode;
      createVNode = module.createVNode;
    }, function (module) {
      Panel = module["default"];
    }, function (module) {
      TextBox = module["default"];
    }, function (module) {
      NumberBox = module["default"];
    }, function (module) {
      Button = module["default"];
    }, function (module) {
      useInvokeBlockAction = module.useInvokeBlockAction;
    }, function (module) {
      BtnType = module.BtnType;
    }],
    execute: (function () {

      function asyncGeneratorStep(gen, resolve, reject, _next, _throw, key, arg) {
        try {
          var info = gen[key](arg);
          var value = info.value;
        } catch (error) {
          reject(error);
          return;
        }
        if (info.done) {
          resolve(value);
        } else {
          Promise.resolve(value).then(_next, _throw);
        }
      }
      function _asyncToGenerator(fn) {
        return function () {
          var self = this,
            args = arguments;
          return new Promise(function (resolve, reject) {
            var gen = fn.apply(self, args);
            function _next(value) {
              asyncGeneratorStep(gen, resolve, reject, _next, _throw, "next", value);
            }
            function _throw(err) {
              asyncGeneratorStep(gen, resolve, reject, _next, _throw, "throw", err);
            }
            _next(undefined);
          });
        };
      }

      var _hoisted_1 = {
        key: 0,
        class: "alert alert-success"
      };
      var _hoisted_2 = {
        key: 1,
        class: "alert alert-danger"
      };
      var _hoisted_3 = createTextVNode(" Save ");
      var script = exports('default', defineComponent({
        name: 'chatbotConfiguration',
        setup(__props) {
          var configuration = ref(null);
          var saveFailed = ref();
          var invokeBlockAction = useInvokeBlockAction();
          function saveData() {
            return _saveData.apply(this, arguments);
          }
          function _saveData() {
            _saveData = _asyncToGenerator(function* () {
              saveFailed.value = undefined;
              var result = yield invokeBlockAction("SaveConfiguration", {
                configuration: configuration.value
              });
              if (result !== null && result !== void 0 && result.isSuccess && result.data !== null) {
                configuration.value = result.data;
                saveFailed.value = false;
              } else {
                saveFailed.value = true;
              }
            });
            return _saveData.apply(this, arguments);
          }
          function loadData() {
            return _loadData.apply(this, arguments);
          }
          function _loadData() {
            _loadData = _asyncToGenerator(function* () {
              try {
                var result = yield invokeBlockAction("GetConfiguration");
                if (result !== null && result !== void 0 && result.isSuccess && result.data !== null) {
                  configuration.value = result.data;
                } else {
                  return null;
                }
              } finally {}
            });
            return _loadData.apply(this, arguments);
          }
          loadData();
          return (_ctx, _cache) => {
            return openBlock(), createBlock(unref(Panel), {
              title: "Chatbot Configuration"
            }, {
              default: withCtx(() => [configuration.value ? (openBlock(), createElementBlock(Fragment, {
                key: 0
              }, [saveFailed.value === false ? (openBlock(), createElementBlock("div", _hoisted_1, "Configuration saved")) : createCommentVNode("v-if", true), saveFailed.value === true ? (openBlock(), createElementBlock("div", _hoisted_2, "Configuration save failed")) : createCommentVNode("v-if", true), createVNode(unref(TextBox), {
                label: "Index Name",
                modelValue: configuration.value.indexName,
                "onUpdate:modelValue": _cache[0] || (_cache[0] = $event => configuration.value.indexName = $event)
              }, null, 8, ["modelValue"]), createVNode(unref(NumberBox), {
                label: "Chunk Size",
                modelValue: configuration.value.chunkSize,
                "onUpdate:modelValue": _cache[1] || (_cache[1] = $event => configuration.value.chunkSize = $event)
              }, null, 8, ["modelValue"]), createVNode(unref(NumberBox), {
                label: "Chunk Overlap",
                modelValue: configuration.value.chunkOverlap,
                "onUpdate:modelValue": _cache[2] || (_cache[2] = $event => configuration.value.chunkOverlap = $event)
              }, null, 8, ["modelValue"]), createVNode(unref(NumberBox), {
                label: "Second Pass Chunk Size",
                modelValue: configuration.value.secondPassChunkSize,
                "onUpdate:modelValue": _cache[3] || (_cache[3] = $event => configuration.value.secondPassChunkSize = $event)
              }, null, 8, ["modelValue"]), createVNode(unref(Button), {
                onClick: _cache[4] || (_cache[4] = $event => saveData()),
                class: "pull-right",
                btnType: unref(BtnType).Primary
              }, {
                default: withCtx(() => [_hoisted_3]),
                _: 1
              }, 8, ["btnType"])], 64)) : createCommentVNode("v-if", true)]),
              _: 1
            });
          };
        }
      }));

      script.__file = "src/chatbotConfiguration.obs";

    })
  };
}));
//# sourceMappingURL=chatbotConfiguration.obs.js.map
